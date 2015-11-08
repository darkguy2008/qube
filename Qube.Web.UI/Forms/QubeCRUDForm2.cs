using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qube.Web.UI
{
    public enum ECRUDFormSwitchMode
    {
        QueryString = 1,
        Postback = 2
    }

    public class QubeCRUDForm2 : QubeFormBase
    {
        private QSManager _qs { get; set; }
        private string VSID { get; set; }

        private string KeyFormMode;

        public string DataId { get; set; }
        public string DataQueryStringNewField { get; set; }
        public string DataQueryStringEditField { get; set; }
        public string DataQueryStringDeleteField { get; set; }

        public EPanelType FormMode { get; set; }
        public ECRUDFormSwitchMode SwitchMode { get; set; }

        public QubeCRUDForm2() : base()
        {
            FormMode = EPanelType.Read;
            SwitchMode = ECRUDFormSwitchMode.QueryString;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _qs = new QSManager(Page.Request);

            KeyFormMode = VSID + "_FormMode";

            if (SwitchMode == ECRUDFormSwitchMode.QueryString)
            {
                if (_qs.Contains(DataQueryStringNewField))
                    FormMode = EPanelType.Create;
                if (_qs.Contains(DataQueryStringEditField))
                {
                    DataId = _qs[DataQueryStringEditField];
                    FormMode = EPanelType.Update;
                }
                if (_qs.Contains(DataQueryStringDeleteField))
                {
                    DataId = _qs[DataQueryStringDeleteField];
                    FormMode = EPanelType.Delete;
                }
            }

            FormMode = ViewState[KeyFormMode] == null ? FormMode : (EPanelType)ViewState[KeyFormMode];

            DebugLine.Write("Test");
            DebugLine.Write(FormMode);
            DebugLine.Write(SwitchMode);

            InvokeFirstLoad();
        }
    }
}
