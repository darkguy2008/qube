using Qube.Extensions;
using System.Web;

namespace Qube.WebServices
{
    class JSONPWebSesrvice : JSONWebService
    {
        protected override void WriteJSON(HttpContext cx, object obj)
        {
            cx.Response.ContentType = "text/javascript";
            cx.Response.Write(string.Format("{0}({1});", qs["callback"], obj.ToJSON()));
        }
    }
}
