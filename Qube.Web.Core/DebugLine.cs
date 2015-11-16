using System.Web;

namespace Qube.Web.Core
{
    public static class DebugLine
    {
        public static int line = 0;

        public static void Write(object obj)
        {
            HttpContext.Current.Response.Write("<div style='position: relative; top: 0; left: 0; border: 1px solid red; background: #fdd; margin-bottom: 1px;'>" + obj.ToString() + "</div>");
            line++;
        }

    }
}
