using Qube.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;

namespace Qube.WebServices
{
    public class JSONWebClient
    {
        public string ServiceURL { get; set; }

        public T MethodGET<T>(string functionName, Dictionary<string, object> args)
        {
            StringBuilder url = new StringBuilder();
            url.Append(ServiceURL);
            url.Append("?op=");
            url.Append(functionName);
            if (args != null && args.Count > 0)
                foreach (KeyValuePair<string, object> kv in args)
                    url.Append("&" + kv.Key + "=" + kv.Value);
            return Serialization.FromJSON<T>(new WebClient() { Encoding = Encoding.UTF8 }.DownloadString(url.ToString()));
        }
        public T MethodPOST<T>(string functionName, Dictionary<string, object> args)
        {
            NameValueCollection data = new NameValueCollection();
            args["op"] = functionName;
            foreach (var kvp in args)
                data.Add(kvp.Key, kvp.Value.ToString());
            return Serialization.FromJSON<T>(Encoding.UTF8.GetString(new WebClient().UploadValues(ServiceURL, "POST", data)));
        }

        public void FixDates(object obj)
        {
            if (obj == null)
                return;

            foreach (Type i in obj.GetType().GetInterfaces())
            {
                if (i.Name.ToLowerInvariant().Trim() == "ilist")
                {
                    var e = obj as IList;
                    if (e != null)
                        foreach (var el in e)
                            FixDates(el);
                }
            }

            foreach (PropertyInfo pi in obj.GetType().GetProperties())
                if (pi.PropertyType == typeof(DateTime))
                    pi.SetValue(obj, ((DateTime)pi.GetValue(obj, null)).ToLocalTime(), null);
        }
    }
}
