using Qube.Globalization;
using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    public class QubeContactForm : QubeFormBase
    {
        public String SendButtonText { get; set; }
        public event QubeFormBaseOperationEventHandler Send;

        public void InvokeSend()
        {
            if (Send != null)
                Send(this, GetFields());
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
            ((QubeContactForm)FormParent).InvokeSend();
        }
    }
}
