using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qube.Web.Core;
using Qube.Extensions;
using System.Reflection;
using System.IO;
using System.Text;
using System.Web.Configuration;

namespace Qube.WebServices
{
    [AttributeUsage(AttributeTargets.All)]
    public class POSTMethod : Attribute
    {
        public POSTMethod() { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class GETMethod : Attribute
    {
        public GETMethod() { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ReturnsDate : Attribute
    {
        public ReturnsDate() { }
    }

    public class JSONWebService : IHttpHandler
    {
        protected QSManager qs;
        public StreamReader Post;
        public HttpRequest Request;
        private HttpContext Context;
        private bool isCustomResponse = false;

        protected virtual void WriteJSON(HttpContext cx, object obj)
        {
            cx.Response.ContentType = "text/plain";
            cx.Response.Write(obj.ToJSON());
        }

        public void ProcessRequest(HttpContext cx)
        {
            Context = cx;
            if (cx.Request.HttpMethod == "GET")
                qs = new QSManager(cx.Request.QueryString);
            else if (cx.Request.HttpMethod == "POST")
                qs = new QSManager(cx.Request.Form);

            cx.Response.ContentEncoding = Encoding.UTF8;

            if (!qs.Contains("op"))
            {
                WriteJSON(cx, new { Result = -1 });
                cx.Response.End();
            }
            else
            {
                string op = qs["op"];
                qs.Remove("op");
                Request = cx.Request;
                using (Post = new StreamReader(cx.Request.InputStream))
                    try
                    {
                        object rv = Run(op, qs);
                        if (!isCustomResponse)
                            WriteJSON(cx, rv);
                    }
                    catch (Exception e)
                    {
                        WriteJSON(cx, new
                        {
                            StatusCode = -999,
                            Message = e.Message + (e.InnerException != null ? (", " + e.InnerException.Message) : string.Empty)
                        });
                    }
            }
        }

        public object Run(string fnName, QSManager qs)
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

        public void CSharpClient()
        {
            CompilationSection compilationSection = (CompilationSection)WebConfigurationManager.GetSection(@"system.web/compilation");
            if (!compilationSection.Debug)
                throw new Exception("Debug parameter in compilation web.config section is not set to True");

            isCustomResponse = true;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("public class WebService : JSONWebClient");
            sb.AppendLine("{");
            sb.AppendLine("    public WebService(string svcUrl) { ServiceURL = svcUrl; }");
            sb.AppendLine("    ");

            List<MethodInfo> lMethods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
            foreach (MethodInfo mi in lMethods)
            {
                string fixDates = string.Empty;
                string method = "MethodGET";
                foreach (var attr in mi.GetCustomAttributes(false))
                {
                    if (attr.GetType() == typeof(POSTMethod))
                        method = "MethodPOST";
                    if (attr.GetType() == typeof(ReturnsDate))
                        fixDates = "FixDates(rv);";
                }

                List<string> lArgs = new List<string>();
                foreach (ParameterInfo pi in mi.GetParameters())
                    lArgs.Add(pi.ToString());

                sb.AppendLine(string.Format("    public T {0}<T>({1})", mi.Name, string.Join(", ", lArgs)));
                sb.AppendLine("    {");
                sb.AppendLine(string.Format("        T rv = {0}<T>(\"{1}\", new Dictionary<string, object>()", method, mi.Name) + " {");

                lArgs = new List<string>();
                foreach (ParameterInfo pi in mi.GetParameters())
                    lArgs.Add("            { " + string.Format("\"{0}\", {0}", pi.Name) + " }");
                sb.AppendLine(string.Join("," + Environment.NewLine, lArgs));
                sb.AppendLine("        });");
                if (!string.IsNullOrEmpty(fixDates))
                    sb.AppendLine("        " + fixDates);
                sb.AppendLine("        return rv;");
                sb.AppendLine("    }");
                sb.AppendLine("    ");
            }

            sb.AppendLine("}");
            Context.Response.Clear();
            Context.Response.ContentType = "text/plain";
            Context.Response.Write(sb.ToString());
        }

        public bool IsReusable { get { return false; } }
    }
}
