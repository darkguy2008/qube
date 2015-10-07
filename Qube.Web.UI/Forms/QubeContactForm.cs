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
using Qube.Extensions;

namespace Qube.Web.UI
{
    public class QubeContactForm : QubeFormBase
    {
        public class QubeContactFormSendArguments
        {
            public Dictionary<string, IQubeFormField> Fields { get; set; }
            public SmtpClient Smtp { get; set; }
            public MailMessage Mail { get; set; }
            public bool Success { get; set; }
            public bool Cancel { get; set; }
        }
        public delegate void QubeContactFormBeforeSendEventHandler(QubeContactForm sender, QubeContactFormSendArguments args);
        public delegate void QubeContactFormAfterSendEventHandler(QubeContactForm sender, QubeContactFormSendArguments args);

        public string SendButtonText { get; set; }
        public event QubeContactFormBeforeSendEventHandler BeforeSend;
        public event QubeContactFormAfterSendEventHandler AfterSend;

        public Encoding EmailEncoding { get; set; }
        public string EmailSubject { get; set; }
        public string EmailFromName { get; set; }
        public string EmailFromAddress { get; set; }
        public string EmailToAddressesCSV { get; set; }
        public string EmailCCAddressesCSV { get; set; }
        public string EmailBCCAddressesCSV { get; set; }
        public string EmailTemplateFile { get; set; }

        public QubeContactForm()
        {
            this.FormLoad += QubeContactForm_Load;
        }

        private void QubeContactForm_Load(object sender, EventArgs e)
        {
            QubeExtensions.ControlFinder<QubeContactPanel> cf = new QubeExtensions.ControlFinder<QubeContactPanel>();
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
            if (!string.IsNullOrEmpty(EmailCCAddressesCSV))
                a.Mail.CC.Add(EmailCCAddressesCSV);
            if(!string.IsNullOrEmpty(EmailBCCAddressesCSV))
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

            foreach (KeyValuePair<string, IQubeFormField> kvp in a.Fields)
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

            btnSend.ID = string.Format("{0}_{1}_{2}", Parent.ID, ID, "btnSend");       
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
