using Qube.Globalization;
using Qube.Web.Core;
using System;
using System.Web.UI.WebControls;
using System.Linq;

namespace Qube.Web.UI
{
    public class QubeStandardForm : QubeFormBase
    {
        public delegate void QubeStandardFormBeforeSubmitEventHandler(QubeStandardForm sender, QubeFormBaseSubmitArguments args);
        public delegate void QubeStandardFormAfterSubmitEventHandler(QubeStandardForm sender, QubeFormBaseSubmitArguments args);

        public String SubmitButtonText { get; set; }
        public event QubeStandardFormBeforeSubmitEventHandler BeforeSubmit;
        public event QubeStandardFormAfterSubmitEventHandler AfterSubmit;

        public QubeStandardForm()
        {
            this.FormLoad += QubeStandardForm_Load;
        }

        private void QubeStandardForm_Load(object Submiter, EventArgs e)
        {
            Extensions.ControlFinder<QubeStandardPanel> cf = new Extensions.ControlFinder<QubeStandardPanel>();
            cf.FindChildControlsRecursive(this);
            cf.FoundControls.First().IsCurrent = true;
        }

        public void DoSubmit()
        {
            QubeFormBaseSubmitArguments a = new QubeFormBaseSubmitArguments();
            a.Fields = GetFields();
            a.Cancel = false;
            a.Success = false;

            if (BeforeSubmit != null)
                BeforeSubmit.Invoke(this, a);
            if (a.Cancel)
                return;

            InvokeSubmit(a);

            if (AfterSubmit != null)
                AfterSubmit.Invoke(this, a);
        }
    }

    public class QubeStandardPanel : QubeFormBasePanel
    {
        private GlobalizedStrings Lang;
        private Button btnSubmit = new Button();

        public QubeStandardPanel()
        {
            LoadControls += QubeStandardPanel_LoadControls;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Lang = new GlobalizedStrings();
            ((QubeStandardForm)FormParent).SubmitButtonText = Lang["CommonSubmit"];

            btnSubmit.ID = String.Format("{0}_{1}_{2}", Parent.ID, ID, "btnSubmit");       
            btnSubmit.CausesValidation = true;            
            btnSubmit.Click += BtnSubmit_Click;
        }

        private void QubeStandardPanel_LoadControls(object Submiter, EventArgs e)
        {
            btnSubmit.Text = ((QubeStandardForm)FormParent).SubmitButtonText;
            Controls.Add(new Panel() { CssClass = "clear" });
            Controls.Add(btnSubmit);
        }

        private void BtnSubmit_Click(object Submiter, EventArgs e)
        {
            if(Page.IsValid)
                ((QubeStandardForm)FormParent).DoSubmit();
        }
    }
}
