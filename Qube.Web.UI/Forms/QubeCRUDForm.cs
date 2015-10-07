using Qube.Extensions;
using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

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
        public string NewButtonText { get; set; }
        public string UpdateButtonText { get; set; }
        public string DeleteButtonText { get; set; }
        public string CancelButtonText { get; set; }
        private string VSID { get; set; }
        public string Key
        {
            get { return (string)ViewState[VSID + "key"]; }
            set { ViewState[VSID + "key"] = value; }
        }

        public delegate void QubeCRUDFormOperationEventHandler(QubeCRUDForm sender, Dictionary<string, IQubeFormField> fields);
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
            QubeExtensions.ControlFinder<QubeCRUDFormPanel> cf = new QubeExtensions.ControlFinder<QubeCRUDFormPanel>();
            cf.FindChildControlsRecursive(this);
            List<QubeCRUDFormPanel> Panels = cf.FoundControls.ToList();

            foreach (QubeCRUDFormPanel p in Panels)
                p.Visible = p.Types.HasFlag(FormMode);

            base.OnPreRender(e);
        }

        public Panel GetCurrentPanel()
        {
            QubeExtensions.ControlFinder<QubeCRUDFormPanel> cf = new QubeExtensions.ControlFinder<QubeCRUDFormPanel>();
            cf.FindChildControlsRecursive(this);
            return cf.FoundControls.Where(x => x.Types.HasFlag(FormMode)).First();
        }

        public Dictionary<string, IQubeFormField> GetFields()
        {
            Panel p = GetCurrentPanel();

            QubeExtensions.ControlFinder<IQubeFormField> cfFields = new QubeExtensions.ControlFinder<IQubeFormField>();
            cfFields.FindChildControlsRecursive(p, false);

            Dictionary<string, IQubeFormField> rv = new Dictionary<string, IQubeFormField>();
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

        public static void FieldsToObject(object dst, Dictionary<string, IQubeFormField> fields)
        {
            PropertyInfo[] pi = dst.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
                if (fields.ContainsKey(p.Name))
                {
                    object v = Convert.ChangeType(fields[p.Name].GetValue<object>(), p.PropertyType);
                    if (p.CanWrite)
                        p.SetValue(dst, v, null);
                }
        }

        public static void ObjectToFields(object src, Dictionary<string, IQubeFormField> fields)
        {
            PropertyInfo[] pi = src.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
                if (fields.ContainsKey(p.Name))
                {
                    object v = p.GetValue(src, null);
                    if (v != null)
                        if (v.GetType() == typeof(DateTime))
                            if (!string.IsNullOrEmpty(fields[p.Name].DataFormatString))
                                v = ((DateTime)v).ToString(fields[p.Name].DataFormatString);
                    fields[p.Name].SetValue(v);
                }
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

            btnNew.ID = string.Format("{0}_{1}_{2}", Parent.ID, ID, "btnCreate");
            btnNew.CausesValidation = false;
            btnNew.Click += BtnCreate_Click;

            btnUpdate.ID = string.Format("{0}_{1}_{2}", Parent.ID, ID, "btnUpdate");
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete.ID = string.Format("{0}_{1}_{2}", Parent.ID, ID, "btnDelete");
            btnDelete.Click += BtnDelete_Click;

            btnCancel.ID = string.Format("{0}_{1}_{2}", Parent.ID, ID, "btnCancel");
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
                    lbField.Controls.Add(new Literal() { Text = ((IQubeFormField)c).DisplayName + ":" });
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
