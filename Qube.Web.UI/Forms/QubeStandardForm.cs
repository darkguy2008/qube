using Qube.Globalization;
using Qube.Web.Core;
using System;
using System.Web.UI.WebControls;
using System.Linq;
using System.Web.UI;
using Qube.Extensions;

namespace Qube.Web.UI
{
    public class QubeStandardForm : QubeFormBase
    {
        public delegate void QubeStandardFormBeforeSubmitEventHandler(QubeStandardForm sender, QubeFormBaseSubmitArguments args);
        public delegate void QubeStandardFormAfterSubmitEventHandler(QubeStandardForm sender, QubeFormBaseSubmitArguments args);

        public bool RenderFieldsOuterDiv { get; set; }
        public string SubmitButtonText { get; set; }
        public bool SubmitEnabled { get; set; }
        public event QubeStandardFormBeforeSubmitEventHandler BeforeSubmit;
        public event QubeStandardFormAfterSubmitEventHandler AfterSubmit;

        public QubeStandardForm()
        {
            this.FormLoad += QubeStandardForm_Load;
            SubmitEnabled = true;
        }

        private void QubeStandardForm_Load(object Submiter, EventArgs e)
        {
            QubeExtensions.ControlFinder<QubeStandardPanel> cf = new QubeExtensions.ControlFinder<QubeStandardPanel>();
            cf.FindChildControlsRecursive(this);
            cf.FoundControls.First().IsCurrent = true;
            cf.FoundControls.First().RenderFieldsOuterDiv = RenderFieldsOuterDiv;
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

        public bool RenderFieldsOuterDiv { get; set; }

        public QubeStandardPanel()
        {
            LoadControls += QubeStandardPanel_LoadControls;
            RenderControls += QubeStandardPanel_RenderControls;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Lang = new GlobalizedStrings();
            if(string.IsNullOrEmpty(((QubeStandardForm)FormParent).SubmitButtonText))
                ((QubeStandardForm)FormParent).SubmitButtonText = Lang["CommonSubmit"];

            btnSubmit.ID = string.Format("{0}_{1}_{2}", Parent.ID, ID, "btnSubmit");       
            btnSubmit.CausesValidation = true;            
            btnSubmit.Click += BtnSubmit_Click;            
        }

        protected override void Render(HtmlTextWriter w)
        {
            base.RenderBeginTag(w);

            foreach (Control c in Controls)
            {
                Panel p = new Panel();
                if (c.GetType().GetInterfaces().Contains(typeof(IQubeFormField)) && c.Parent.GetType() == typeof(QubeStandardPanel))
                {
                    IQubeFormField fld = c as IQubeFormField;

                    if(RenderFieldsOuterDiv)
                        p.RenderBeginTag(w);    
                    HtmlCustomControl lbField = new HtmlCustomControl("label");
                    lbField.Attributes["for"] = c.ClientID;
                    lbField.Controls.Add(new Literal() { Text = ((IQubeFormField)c).DisplayName + ":" });
                    HtmlCustomControl span = new HtmlCustomControl("span");
                    if (!string.IsNullOrEmpty(fld.DisplayName))
                    {
                        lbField.RenderControl(w);
                        span.RenderBeginTag(w);
                    }
                    c.RenderControl(w);
                    if (!string.IsNullOrEmpty(fld.DisplayName))
                        span.RenderEndTag(w);
                    if (RenderFieldsOuterDiv)
                        p.RenderEndTag(w);
                    continue;
                }
                else if (c.GetType() == typeof(Captcha))
                {
                    if (RenderFieldsOuterDiv)
                        p.RenderBeginTag(w);
                    HtmlCustomControl lbField = new HtmlCustomControl("label");
                    lbField.Controls.Add(new Literal() { Text = "&nbsp;" });
                    HtmlCustomControl span = new HtmlCustomControl("span");
                    lbField.RenderControl(w);
                    span.RenderBeginTag(w);
                    c.RenderControl(w);
                    span.RenderEndTag(w);
                    if (RenderFieldsOuterDiv)
                        p.RenderEndTag(w);
                }
                else
                    c.RenderControl(w);
            }

            base.RenderEndTag(w);
        }

        private void QubeStandardPanel_LoadControls(object Submiter, EventArgs e)
        {
            btnSubmit.Text = ((QubeStandardForm)FormParent).SubmitButtonText;
            Controls.Add(new Panel() { CssClass = "clear" });
            Controls.Add(btnSubmit);
        }

        private void QubeStandardPanel_RenderControls(object sender, EventArgs e)
        {
            btnSubmit.Enabled = ((QubeStandardForm)Parent).SubmitEnabled;
        }

        private void BtnSubmit_Click(object Submiter, EventArgs e)
        {
            if(Page.IsValid)
                ((QubeStandardForm)FormParent).DoSubmit();
        }
    }
}
