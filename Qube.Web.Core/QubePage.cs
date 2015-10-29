using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Linq;

namespace Qube.Web.Core
{
    public class QubePage : Page
    {
        protected override void Render(HtmlTextWriter w)
        {
            string set = WebConfigurationManager.AppSettings["QubeSettings"];
            if (set.ToLowerInvariant().Trim().Split(',').Contains("removewhitespace"))
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(new StringWriter()))
                {
                    base.Render(hw);
                    string html = hw.InnerWriter.ToString();
                    html = Regex.Replace(html, @"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}", string.Empty);
                    html = Regex.Replace(html, @"[ \f\r\t\v]?([\n\xFE\xFF/{}[\];,<>*%&|^!~?:=])[\f\r\t\v]?", "$1");
                    html = html.Replace(";\n", ";");
                    html = HttpUtility.HtmlDecode(html);
                    w.Write(html.Trim());
                }
            }
        }
    }
}
