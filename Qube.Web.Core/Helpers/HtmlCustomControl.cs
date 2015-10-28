using System.Web.UI;
using System.Web.UI.WebControls;

namespace Qube.Web.Core
{
    public class HtmlCustomControl : Panel
    {
        private string _tag;
        public HtmlCustomControl(string tagname) { _tag = tagname; }
        public HtmlCustomControl(string tagname, string text) { _tag = tagname; if (!string.IsNullOrEmpty(text)) Controls.Add(new Literal() { Text = text }); }
        protected override string TagName { get { return _tag; } }
        protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Unknown; } }
    }
}
