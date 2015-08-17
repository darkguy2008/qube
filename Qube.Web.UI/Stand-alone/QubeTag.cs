using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    public class QubeTag : Label
    {
        public enum TagType
        {
            Success = 1,
            Error,
            Info,
            Warning
        }
        public TagType Type { get; set; }

        public QubeTag()
        {
            Type = TagType.Info;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            CssClass = "tag " + Enum.GetName(typeof(TagType), Type).ToLowerInvariant().Trim();
            base.Render(writer);
        }
    }
}
