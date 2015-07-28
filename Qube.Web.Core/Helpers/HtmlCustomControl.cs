using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Qube.Web.Core
{
    public class HtmlCustomControl : Panel
    {
        private String _tag;
        public HtmlCustomControl(String tagname) { _tag = tagname; }
        protected override string TagName { get { return _tag; } }
        protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Unknown; } }
    }
}
