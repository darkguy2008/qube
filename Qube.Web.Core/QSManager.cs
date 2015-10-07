using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Qube.Web.Core
{

    public class QSManager
    {
        private Dictionary<string, string> _qs = new Dictionary<string, string>();

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
            foreach (string key in qs.Keys)
                if (key == null)
                    _qs[qs[key]] = null;
                else
                    _qs[key] = qs[key];
        }

        public string this[string key]
        {
            get { return _qs[key].ToString() ?? null; }
        }

        public bool Contains(params string[] keys)
        {
            foreach (string s in keys)
                if (!_qs.ContainsKey(s))
                    return false;
            return true;
        }

        public void Remove(string key)
        {
            _qs.Remove(key);
        }
    }

}