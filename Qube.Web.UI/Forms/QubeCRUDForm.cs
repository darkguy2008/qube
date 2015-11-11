using Qube.Extensions;
using Qube.Globalization;
using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    [Flags]
    public enum ECRUDPanelType
    {
        Create = 0x1,
        Read = 0x2,
        Update = 0x4,
        Delete = 0x8
    }

    public enum ECRUDFormSwitchMode
    {
        QueryString = 1,
        Postback = 2
    }

    public class QubeCRUDForm : QubeFormBase
    {
        public class QubeCRUDFormActionArguments
        {
            public Dictionary<string, IQubeFormField> Fields { get; set; }
            public bool AbortCancelDefaultBehaviour { get; set; }
        }
        private GlobalizedStrings Lang = new GlobalizedStrings();
        private QSManager _qs { get; set; }
        private string VSID { get; set; }

        private string KeyFormMode;

        public string DataId { get; set; }
        public string DataQueryStringNewField { get; set; }
        public string DataQueryStringEditField { get; set; }
        public string DataQueryStringDeleteField { get; set; }

        public ECRUDPanelType FormMode { get; set; }
        public ECRUDFormSwitchMode SwitchMode { get; set; }

        private Button btnNew = new Button();
        private Button btnSubmit = new Button();
        private Button btnDelete = new Button();
        private Button btnCancel = new Button();
        public string NewButtonText { get; set; }
        public string SubmitButtonText { get; set; }
        public string DeleteButtonText { get; set; }
        public string CancelButtonText { get; set; }
        public bool AllowCreating { get; set; }
        public bool AllowDeleting { get; set; }
        public bool AllowSaving { get; set; }
        public bool AllowCancelling { get; set; }

        public delegate void QubeCRUDFormActionEventHandler(QubeCRUDForm sender, ref QubeCRUDFormActionArguments args);
        public delegate void QubeCRUDFormOperationEventHandler(QubeCRUDForm sender, Dictionary<string, IQubeFormField> fields);
        public event QubeCRUDFormActionEventHandler Inserting;
        public event QubeCRUDFormActionEventHandler Updating;
        public event QubeCRUDFormActionEventHandler Deleting;
        public event QubeCRUDFormActionEventHandler Cancelling;

        public QubeCRUDForm() : base()
        {
            FormMode = ECRUDPanelType.Read;
            SwitchMode = ECRUDFormSwitchMode.QueryString;

            NewButtonText = Lang["CommonNew"];
            SubmitButtonText = Lang["CommonSave"];
            DeleteButtonText = Lang["CommonDelete"];
            CancelButtonText = Lang["CommonCancel"];

            AllowSaving = true;
            AllowCreating = true;
            AllowDeleting = true;
            AllowCancelling = true;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _qs = new QSManager(Page.Request);

            KeyFormMode = VSID + "_FormMode";
            btnNew.Text = NewButtonText;
            btnSubmit.Text = SubmitButtonText;
            btnDelete.Text = DeleteButtonText;
            btnCancel.Text = CancelButtonText;

            btnNew.ID = string.Format("{0}_{1}", ID, "btnCreate");
            btnNew.CausesValidation = false;
            btnNew.Click += BtnCreate_Click;

            btnSubmit.ID = string.Format("{0}_{1}", ID, "btnSave");
            btnSubmit.Click += btnSubmit_Click;

            btnDelete.ID = string.Format("{0}_{1}", ID, "btnDelete");
            btnDelete.Click += btnSubmit_Click;

            btnCancel.ID = string.Format("{0}_{1}", ID, "btnCancel");
            btnCancel.CausesValidation = false;
            btnCancel.Click += BtnCancel_Click;

            if (SwitchMode == ECRUDFormSwitchMode.QueryString)
            {
                if (_qs.Contains(DataQueryStringNewField))
                    FormMode = ECRUDPanelType.Create;
                if (_qs.Contains(DataQueryStringEditField))
                {
                    DataId = _qs[DataQueryStringEditField];
                    FormMode = ECRUDPanelType.Update;
                }
                if (_qs.Contains(DataQueryStringDeleteField))
                {
                    DataId = _qs[DataQueryStringDeleteField];
                    FormMode = ECRUDPanelType.Delete;
                }
            }

            FormMode = ViewState[KeyFormMode] == null ? FormMode : (ECRUDPanelType)ViewState[KeyFormMode];
            ViewState[KeyFormMode] = FormMode;

            foreach (QubeCRUDFormPanel p in GetAllPanels())
            {
                p.Visible = false;
                p.IsCurrent = false;
            }

            GetPanel(FormMode).Visible = true;
            GetPanel(FormMode).IsCurrent = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            QubeCRUDFormPanel panel = GetPanel(FormMode);
            panel.Controls.Add(new Panel() { CssClass = "clear" });

            if (panel.Types.HasFlag(ECRUDPanelType.Read) && AllowCreating)
                panel.Controls.Add(btnNew);

            if ( (panel.Types.HasFlag(ECRUDPanelType.Create) || panel.Types.HasFlag(ECRUDPanelType.Update)) && AllowSaving)
                panel.Controls.Add(btnSubmit);

            if (panel.Types.HasFlag(ECRUDPanelType.Delete) && AllowDeleting)
                panel.Controls.Add(btnDelete);

            if (!panel.Types.HasFlag(ECRUDPanelType.Read) && AllowCancelling)
                panel.Controls.Add(btnCancel);
        }

        protected QubeCRUDFormPanel GetPanel(ECRUDPanelType type)
        {
            QubeExtensions.ControlFinder<QubeCRUDFormPanel> cf = new QubeExtensions.ControlFinder<QubeCRUDFormPanel>();
            cf.FindChildControlsRecursive(this, true);
            return cf.FoundControls.Where(x => x.Types.HasFlag(type)).First();
        }

        protected IEnumerable<QubeCRUDFormPanel> GetAllPanels()
        {
            QubeExtensions.ControlFinder<QubeCRUDFormPanel> cf = new QubeExtensions.ControlFinder<QubeCRUDFormPanel>();
            cf.FindChildControlsRecursive(this, true);
            return cf.FoundControls;
        }

        public void SetMode(ECRUDPanelType mode, object argument = null)
        {
            if (SwitchMode == ECRUDFormSwitchMode.QueryString)
            {
                string field = String.Empty;
                if (mode == ECRUDPanelType.Create)
                {
                    _qs.Remove(DataQueryStringEditField);
                    _qs.Remove(DataQueryStringDeleteField);
                    field = DataQueryStringNewField;
                }
                if (mode == ECRUDPanelType.Update)
                {
                    _qs.Remove(DataQueryStringNewField);
                    _qs.Remove(DataQueryStringDeleteField);
                    field = DataQueryStringEditField;
                }
                if (mode == ECRUDPanelType.Delete)
                {
                    _qs.Remove(DataQueryStringNewField);
                    _qs.Remove(DataQueryStringEditField);
                    field = DataQueryStringDeleteField;
                }
                if (argument != null)
                    _qs[field] = argument.ToString();
                else
                    _qs[field] = null;

                if (mode == ECRUDPanelType.Read)
                {
                    _qs.Remove(DataQueryStringNewField);
                    _qs.Remove(DataQueryStringEditField);
                    _qs.Remove(DataQueryStringDeleteField);
                }

                Page.Response.Redirect(_qs.Build(Page.Request.Url));
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            SetMode(ECRUDPanelType.Create);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;
            QubeCRUDFormActionArguments args = new QubeCRUDFormActionArguments()
            {
                Fields = GetFields(),
                AbortCancelDefaultBehaviour = false
            };
            if (FormMode == ECRUDPanelType.Create)
                if (Inserting != null)
                    Inserting(this, ref args);
            if (FormMode == ECRUDPanelType.Update)
                if (Updating != null)
                    Updating(this, ref args);
            if (FormMode == ECRUDPanelType.Delete)
                if (Deleting != null)
                    Deleting(this, ref args);
            if(!args.AbortCancelDefaultBehaviour)
                SetMode(ECRUDPanelType.Read);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (Cancelling != null)
            {
                QubeCRUDFormActionArguments args = new QubeCRUDFormActionArguments()
                {
                    Fields = GetFields(),
                    AbortCancelDefaultBehaviour = false
                };
                Cancelling(this, ref args);
                if(!args.AbortCancelDefaultBehaviour)
                    SetMode(ECRUDPanelType.Read);
            }
        }
    }

    public class QubeCRUDFormPanel : QubeFormBasePanel
    {
        public ECRUDPanelType Types { get; set; }
        private new QubeCRUDForm FormParent;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FormParent = Parent as QubeCRUDForm;
        }
    }
}
