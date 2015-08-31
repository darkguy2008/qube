using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Qube.Web.Core
{

    public class QSManager
    {
        private Dictionary<String, String> _qs = new Dictionary<String, String>();

        public QSManager(HttpContext cx)
        {
            Load(cx.Request.QueryString);
        }

        public QSManager(HttpRequest rq)
        {
            Load(rq.QueryString);
        }

        public QSManager(NameValueCollection qs)
        {
            Load(qs);
        }

        private void Load(NameValueCollection qs)
        {
            foreach (String key in qs.Keys)
                if (key == null)
                    _qs[qs[key]] = null;
                else
                    _qs[key] = qs[key];
        }

        public String this[String key]
        {
            get { return _qs[key].ToString() ?? null; }
        }

        public bool Contains(params String[] keys)
        {
            foreach (String s in keys)
                if (!_qs.ContainsKey(s))
                    return false;
            return true;
        }

        public void Remove(String key)
        {
            _qs.Remove(key);
        }
    }

}