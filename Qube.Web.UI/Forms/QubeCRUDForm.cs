using Qube.Extensions;
using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public class QubeCRUDForm : QubeFormBase
    {
        private QSManager _qs { get; set; }
        public object DataId { get; set; }
        public string DataQuerystringField { get; set; }
        public string DataQuerystringDeleteField { get; set; }

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
        public bool RedirectOnCancel = true;
        public bool RedirectOnSubmit = true;
        public bool AllowInserting { get; set; }

        public delegate void QubeCRUDFormOperationEventHandler(QubeCRUDForm sender, Dictionary<string, IQubeFormField> fields);
        public event QubeCRUDFormOperationEventHandler Inserting;
        public event QubeCRUDFormOperationEventHandler Updating;
        public event QubeCRUDFormOperationEventHandler Deleting;
        public event QubeCRUDFormOperationEventHandler Cancelling;
        public event EventHandler ModeChanged;

        public QubeCRUDForm() : base()
        {
            FormMode = EPanelType.Read;
            AllowInserting = true;
            NewButtonText = "Nuevo...";
            UpdateButtonText = "Guardar";
            DeleteButtonText = "Eliminar";
            CancelButtonText = "Cancelar";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _qs = new QSManager(Page.Request);
            if (!String.IsNullOrEmpty(DataQuerystringField))
            {
                if (_qs.Contains(DataQuerystringField))
                {
                    DataId = _qs[DataQuerystringField];
                    FormMode = EPanelType.Update;
                }
                if (_qs.Contains(DataQuerystringDeleteField))
                    FormMode = EPanelType.Delete;
            }

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

        public void InvokeInserting()
        {
            if (Inserting != null)
                Inserting(this, GetFields());
            if (RedirectOnSubmit)
                Page.Response.Redirect(Page.Request.Path);
        }

        public void InvokeUpdating()
        {
            if (Updating != null)
                Updating(this, GetFields());
            if (RedirectOnSubmit)
                Page.Response.Redirect(Page.Request.Path);
        }

        public void InvokeDeleting()
        {
            if (Deleting != null)
                Deleting(this, GetFields());
            if (RedirectOnSubmit)
                Page.Response.Redirect(Page.Request.Path);
        }

        public void InvokeCancelling()
        {
            if (Cancelling != null)
                Cancelling(this, GetFields());
            else
                if (RedirectOnCancel)
                    Page.Response.Redirect(Page.Request.Path);
                else
                    SetMode(EPanelType.Read);
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

        public override Panel GetCurrentPanel()
        {
            QubeExtensions.ControlFinder<QubeCRUDFormPanel> cf = new QubeExtensions.ControlFinder<QubeCRUDFormPanel>();
            cf.FindChildControlsRecursive(this, false);
            foreach (QubeCRUDFormPanel p in cf.FoundControls)
                if (p.Types.HasFlag(FormMode))
                {
                    p.IsCurrent = true;
                    break;
                }
            return base.GetCurrentPanel();
        }

    }

    public class QubeCRUDFormPanel : QubeFormBasePanel
    {
        public EPanelType Types { get; set; }
        private Button btnNew = new Button();
        private Button btnUpdate = new Button();
        private Button btnDelete = new Button();
        private Button btnCancel = new Button();
        private new QubeCRUDForm FormParent;

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

            if (Types.HasFlag(EPanelType.Read) && FormParent.AllowInserting)
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
