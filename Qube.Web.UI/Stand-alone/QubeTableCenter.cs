using Qube.Web.Core;
using System.Web.UI;

namespace Qube.Web.UI
{
    public class QubeTableCenter : HtmlCustomControl
    {

        public QubeTableCenter() : base("table") { }

        protected override void Render(HtmlTextWriter w)
        {
            HtmlCustomControl topRow = new HtmlCustomControl("tr");
            for(int i = 1; i <= 3; i++)
                topRow.Controls.Add(new HtmlCustomControl("td"));

            HtmlCustomControl centerRow = new HtmlCustomControl("tr");
            for (int i = 1; i <= 3; i++)
                centerRow.Controls.Add(new HtmlCustomControl("td"));

            HtmlCustomControl bottomRow = new HtmlCustomControl("tr");
            for (int i = 1; i <= 3; i++)
                bottomRow.Controls.Add(new HtmlCustomControl("td"));

            base.RenderBeginTag(w);
            topRow.RenderControl(w);
            centerRow.Controls[0].RenderControl(w);
            ((HtmlCustomControl)centerRow.Controls[1]).RenderBeginTag(w);
            base.RenderContents(w);
            ((HtmlCustomControl)centerRow.Controls[1]).RenderEndTag(w);
            centerRow.Controls[2].RenderControl(w);
            bottomRow.RenderControl(w);
            base.RenderEndTag(w);
        }

    }
}
