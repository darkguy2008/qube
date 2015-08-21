using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Qube.Web.Core;
using Qube.Extensions;
using System.Reflection;

namespace Qube.WebServices
{
    public class JSONWebService : IHttpHandler
    {
        private QSManager qs;

        private void WriteJSON(HttpContext cx, object obj)
        {
            cx.Response.ContentType = "text/plain";
            cx.Response.Write(obj.ToJSON());
        }

        public void ProcessRequest(HttpContext cx)
        {
            qs = new QSManager(cx.Request.QueryString);
            if (!qs.Contains("op"))
            {
                WriteJSON(cx, new { Result = -1 });
                cx.Response.End();
            }
            else
            {
                String op = qs["op"];
                qs.Remove("op");

                try
                {
                    WriteJSON(cx, Run(op, qs));
                }
                catch (Exception e)
                {
                    WriteJSON(cx, new
                    {
                        StatusCode = 3,
                        Message = e.Message + (e.InnerException != null ? (", " + e.InnerException.Message) : String.Empty)
                    });
                }
            }
        }

        public object Run(String fnName, QSManager qs)
        {
            List<String> args = new List<String>();
            MethodInfo fn = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name.ToLowerInvariant().Trim() == fnName.ToLowerInvariant().Trim()).SingleOrDefault();
            if (fn == null)
                throw new Exception("Invalid function name");

            foreach (ParameterInfo pi in fn.GetParameters())
            {
                if (!qs.Contains(pi.Name))
                    throw new Exception("The parameter " + pi.Name + " does not exist");
                args.Add(qs[pi.Name]);
            }

            return fn.Invoke(this, args.ToArray());
        }

        public bool IsReusable { get { return false; } }
    }
}
