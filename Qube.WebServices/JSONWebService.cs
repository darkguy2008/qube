using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qube.Web.Core;
using Qube.Extensions;
using System.Reflection;
using System.IO;

namespace Qube.WebServices
{
    public class JSONWebService : IHttpHandler
    {
        protected QSManager qs;
        public StreamReader Post;
        public HttpRequest Request;

        protected virtual void WriteJSON(HttpContext cx, object obj)
        {
            cx.Response.ContentType = "text/plain";
            cx.Response.Write(obj.ToJSON());
        }

        public void ProcessRequest(HttpContext cx)
        {
            if (cx.Request.HttpMethod == "GET")
                qs = new QSManager(cx.Request.QueryString);
            else if (cx.Request.HttpMethod == "POST")
                qs = new QSManager(cx.Request.Form);

            if (!qs.Contains("op"))
            {
                WriteJSON(cx, new { Result = -1 });
                cx.Response.End();
            }
            else
            {
                String op = qs["op"];
                qs.Remove("op");
                Request = cx.Request;
                using (Post = new StreamReader(cx.Request.InputStream))
                    try
                    {
                        WriteJSON(cx, Run(op, qs));
                    }
                    catch (Exception e)
                    {
                        WriteJSON(cx, new
                        {
                            StatusCode = -999,
                            Message = e.Message + (e.InnerException != null ? (", " + e.InnerException.Message) : String.Empty)
                        });
                    }
            }
        }

        public object Run(String fnName, QSManager qs)
        {
            List<object> args = new List<object>();
            MethodInfo fn = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name.ToLowerInvariant().Trim() == fnName.ToLowerInvariant().Trim()).SingleOrDefault();
            if (fn == null)
                throw new Exception("Invalid function name");

            foreach (ParameterInfo pi in fn.GetParameters())
            {
                if (!qs.Contains(pi.Name))
                    throw new Exception("The parameter " + pi.Name + " does not exist");
                args.Add(Convert.ChangeType(qs[pi.Name], pi.ParameterType));
            }

            return fn.Invoke(this, args.ToArray());
        }

        public bool IsReusable { get { return false; } }
    }
}
