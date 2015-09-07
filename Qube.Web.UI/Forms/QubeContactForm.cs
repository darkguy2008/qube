using Qube.Globalization;
using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Linq;

namespace Qube.Web.UI
{
    public class QubeContactForm : QubeFormBase
    {
        public class QubeContactFormSendArguments
        {
            public Dictionary<String, IQubeFormField> Fields { get; set; }
            public SmtpClient Smtp { get; set; }
            public MailMessage Mail { get; set; }
            public bool Success { get; set; }
            public bool Cancel { get; set; }
        }
        public delegate void QubeContactFormBeforeSendEventHandler(QubeContactForm sender, QubeContactFormSendArguments args);
        public delegate void QubeContactFormAfterSendEventHandler(QubeContactForm sender, QubeContactFormSendArguments args);

        public String SendButtonText { get; set; }
        public event QubeContactFormBeforeSendEventHandler BeforeSend;
        public event QubeContactFormAfterSendEventHandler AfterSend;

        public Encoding EmailEncoding { get; set; }
        public String EmailSubject { get; set; }
        public String EmailFromName { get; set; }
        public String EmailFromAddress { get; set; }
        public String EmailToAddressesCSV { get; set; }
        public String EmailCCAddressesCSV { get; set; }
        public String EmailBCCAddressesCSV { get; set; }
        public String EmailTemplateFile { get; set; }

        public QubeContactForm()
        {
            this.FormLoad += QubeContactForm_Load;
        }

        private void QubeContactForm_Load(object sender, EventArgs e)
        {
            Extensions.ControlFinder<QubeContactPanel> cf = new Extensions.ControlFinder<QubeContactPanel>();
            cf.FindChildControlsRecursive(this);
            cf.FoundControls.First().IsCurrent = true;
        }

        public void SendEmail()
        {
            QubeContactFormSendArguments a = new QubeContactFormSendArguments();
            a.Fields = GetFields();
            a.Cancel = false;
            a.Success = false;
            a.Smtp = new SmtpClient();
            a.Mail = new MailMessage();
            a.Mail.From = new MailAddress(EmailFromAddress, EmailFromName);
            a.Mail.To.Add(EmailToAddressesCSV);
            if (!String.IsNullOrEmpty(EmailCCAddressesCSV))
                a.Mail.CC.Add(EmailCCAddressesCSV);
            if(!String.IsNullOrEmpty(EmailBCCAddressesCSV))
                a.Mail.Bcc.Add(EmailBCCAddressesCSV);
            a.Mail.Subject = EmailSubject;
            a.Mail.SubjectEncoding = EmailEncoding;
            a.Mail.IsBodyHtml = true;
            a.Mail.BodyEncoding = EmailEncoding;
            a.Mail.Body = File.ReadAllText(HttpContext.Current.Server.MapPath(EmailTemplateFile));

            if (BeforeSend != null)
                BeforeSend.Invoke(this, a);
            if (a.Cancel)
                return;

            foreach (KeyValuePair<String, IQubeFormField> kvp in a.Fields)
                a.Mail.Body = a.Mail.Body.Replace("{{" + kvp.Value.DataField + "}}", kvp.Value.GetFormattedValue());

            try
            {
                a.Smtp.Send(a.Mail);
                a.Success = true;
            }
            catch (Exception) { }

            if (AfterSend != null)
                AfterSend.Invoke(this, a);
        }
    }

    public class QubeContactPanel : QubeFormBasePanel
    {
        private GlobalizedStrings Lang;
        private Button btnSend = new Button();

        public QubeContactPanel()
        {
            LoadControls += QubeContactPanel_LoadControls;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Lang = new GlobalizedStrings();
            ((QubeContactForm)FormParent).SendButtonText = Lang["CommonSend"];

            btnSend.ID = String.Format("{0}_{1}_{2}", Parent.ID, ID, "btnSend");       
            btnSend.CausesValidation = true;            
            btnSend.Click += BtnSend_Click;
        }

        private void QubeContactPanel_LoadControls(object sender, EventArgs e)
        {
            btnSend.Text = ((QubeContactForm)FormParent).SendButtonText;
            Controls.Add(new Panel() { CssClass = "clear" });
            Controls.Add(btnSend);
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if(Page.IsValid)
                ((QubeContactForm)FormParent).SendEmail();
        }
    }
}
