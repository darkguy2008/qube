using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI;
using Qube.Extensions;

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
            // TODO: Refactor algorithm into function?
            Control c = Page.Master;
            MasterPage m = Page.Master;
            while (m.Master != null)
                m = m.Master;
            if (c == null)
                c = Page;
            ((HtmlControl)QubeExtensions.FindControlRecursive(c, "body")).Attributes.Add("onload", "setTimeout( function(){ window.location.assign('mailto:" + Address + "'); }, 500);");
        }
    }
}
