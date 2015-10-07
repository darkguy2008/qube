using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    public class MailTo : LinkButton
    {
        public string Address { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.CausesValidation = false;
            this.Click += MailTo_Click;            
        }

        private void MailTo_Click(object sender, EventArgs e)
        {
            ((HtmlControl)Page.Master.FindControl("body")).Attributes.Add("onload", "setTimeout( function(){ window.location.assign('mailto:" + Address + "'); }, 500);");
        }
    }
}
