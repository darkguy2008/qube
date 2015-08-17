using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    public class MailTo : LinkButton
    {
        public String Address { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Click += MailTo_Click;
        }

        private void MailTo_Click(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Redirect("mailto:" + Address);
        }
    }
}
