using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Qube.Web.Core.Extensions;

namespace Qube.Web.UI
{
    [Flags]
    public enum EPanelType
    {
        Create = 0x1,
        Read = 0x2,
        Update = 0x4,
        Delete = 0x8
    }

    public class QubeCRUDForm : HtmlCustomControl
    {
        public EPanelType FormMode { get; set; }
        public String NewButtonText { get; set; }
        public String UpdateButtonText { get; set; }
        public String DeleteButtonText { get; set; }
        public String CancelButtonText { get; set; }
        private String VSID { get; set; }
        public String Key
        {
            get { return (String)ViewState[VSID + "key"]; }
            set { ViewState[VSID + "key"] = value; }
        }

        public delegate void QubeCRUDFormOperationEventHandler(QubeCRUDForm sender, Dictionary<String, IQubeFormField> fields);
        public event QubeCRUDFormOperationEventHandler FirstLoad;
        public event QubeCRUDFormOperationEventHandler Inserting;
        public event QubeCRUDFormOperationEventHandler Updating;
        public event QubeCRUDFormOperationEventHandler Deleting;
        public event QubeCRUDFormOperationEventHandler Cancelling;
        public event EventHandler ModeChanged;

        public QubeCRUDForm() : base("div")
        {
            FormMode = EPanelType.Read;
            NewButtonText = "Nuevo...";
            UpdateButtonText = "Guardar";
            DeleteButtonText = "Eliminar";
            CancelButtonText = "Cancelar";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FormMode = ViewState[VSID + "_FormMode"] == null ? FormMode : (EPanelType)ViewState[VSID + "_FormMode"];
            InvokeFirstLoad();
        }

        protected override void OnPreRender(EventArgs e)
        {
            ControlFinder<QubeCRUDFormPanel> cf = new ControlFinder<QubeCRUDFormPanel>();
            cf.FindChildControlsRecursive(this);
            List<QubeCRUDFormPanel> Panels = cf.FoundControls.ToList();

            foreach (QubeCRUDFormPanel p in Panels)
                p.Visible = p.Types.HasFlag(FormMode);

            base.OnPreRender(e);
        }

        public Panel GetCurrentPanel()
        {
            ControlFinder<QubeCRUDFormPanel> cf = new ControlFinder<QubeCRUDFormPanel>();
            cf.FindChildControlsRecursive(this);
            return cf.FoundControls.Where(x => x.Types.HasFlag(FormMode)).First();
        }

        public Dictionary<String, IQubeFormField> GetFields()
        {
            Panel p = GetCurrentPanel();

            ControlFinder<IQubeFormField> cfFields = new ControlFinder<IQubeFormField>();
            cfFields.FindChildControlsRecursive(p, false);

            Dictionary<String, IQubeFormField> rv = new Dictionary<String, IQubeFormField>();
            foreach (Control c in cfFields.FoundControls)
                rv[((IQubeFormField)c).DataField] = c as IQubeFormField;

            return rv;
        }

        public void InvokeInserting()
        {
            if (Inserting != null)
                Inserting(this, GetFields());
        }

        public void InvokeUpdating()
        {
            if (Updating != null)
                Updating(this, GetFields());
        }

        public void InvokeDeleting()
        {
            if (Deleting != null)
                Deleting(this, GetFields());
        }

        public void InvokeCancelling()
        {
            if (Cancelling != null)
                Cancelling(this, GetFields());
            else
                SetMode(EPanelType.Read);
        }

        public void InvokeFirstLoad()
        {
            if (FirstLoad != null)
                FirstLoad(this, GetFields());
        }

        public void InvokeModeChanged()
        {
            if (ModeChanged != null)
                ModeChanged(this, new EventArgs());
        }

        public void SetMode(EPanelType mode)
        {
            FormMode = mode;
            ViewState[VSID + "_FormMode"] = mode;
            InvokeModeChanged();
        }

        public static void FieldsToObject(object dst, Dictionary<String, IQubeFormField> fields)
        {
            PropertyInfo[] pi = dst.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
            {
                if(fields.ContainsKey(p.Name))
                    if(p.CanWrite)
                        p.SetValue(dst, Convert.ChangeType(fields[p.Name].GetValue<object>(), p.PropertyType), null);
            }
        }

        public static void ObjectToFields(object src, Dictionary<String, IQubeFormField> fields)
        {
            PropertyInfo[] pi = src.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
                if (fields.ContainsKey(p.Name))
                    fields[p.Name].SetValue(p.GetValue(src, null));
        }
    }

    public class QubeCRUDFormPanel : HtmlCustomControl
    {
        public EPanelType Types { get; set; }
        private Button btnNew = new Button();
        private Button btnUpdate = new Button();
        private Button btnDelete = new Button();
        private Button btnCancel = new Button();
        private QubeCRUDForm FormParent;

        public QubeCRUDFormPanel() : base("div")
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            btnNew.ID = String.Format("{0}_{1}_{2}", Parent.ID, ID, "btnCreate");
            btnNew.CausesValidation = false;
            btnNew.Click += BtnCreate_Click;

            btnUpdate.ID = String.Format("{0}_{1}_{2}", Parent.ID, ID, "btnUpdate");
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete.ID = String.Format("{0}_{1}_{2}", Parent.ID, ID, "btnDelete");
            btnDelete.Click += BtnDelete_Click;

            btnCancel.ID = String.Format("{0}_{1}_{2}", Parent.ID, ID, "btnCancel");
            btnCancel.CausesValidation = false;
            btnCancel.Click += BtnCancel_Click;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FormParent = Parent as QubeCRUDForm;

            btnNew.Text = FormParent.NewButtonText;
            btnUpdate.Text = FormParent.UpdateButtonText;
            btnDelete.Text = FormParent.DeleteButtonText;
            btnCancel.Text = FormParent.CancelButtonText;

            Controls.Add(new Panel() { CssClass = "clear" });

            if (Types.HasFlag(EPanelType.Read))
                Controls.Add(btnNew);

            if (Types.HasFlag(EPanelType.Create) || Types.HasFlag(EPanelType.Update))
                Controls.Add(btnUpdate);

            if (Types.HasFlag(EPanelType.Delete))
                Controls.Add(btnDelete);

            if(!Types.HasFlag(EPanelType.Read))
                Controls.Add(btnCancel);
        }

        protected override void Render(HtmlTextWriter w)
        {
            base.RenderBeginTag(w);

            foreach(Control c in Controls)
            {
                if (c.GetType().GetInterfaces().Contains(typeof(IQubeFormField)))
                {
                    HtmlCustomControl lbField = new HtmlCustomControl("label");
                    lbField.Attributes["for"] = c.ClientID;
                    lbField.Controls.Add(new Literal() { Text = ((IQubeFormField)c).FieldName + ":" });
                    lbField.RenderControl(w);
                    c.RenderControl(w);
                    continue;
                }
                c.RenderControl(w);
            }

            base.RenderEndTag(w);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            ((QubeCRUDForm)Parent).SetMode(EPanelType.Create);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (FormParent.FormMode == EPanelType.Create)
                FormParent.InvokeInserting();
            if (FormParent.FormMode == EPanelType.Update)
                FormParent.InvokeUpdating();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            FormParent.InvokeDeleting();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            FormParent.InvokeCancelling();            
        }
    }
}
