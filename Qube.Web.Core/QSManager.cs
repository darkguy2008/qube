using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Qube.Web.Core
{

    public class QSManager
    {
        private Dictionary<String, String> _qs = new Dictionary<String, String>();

        public QSManager(NameValueCollection qs)
        {
            foreach (String key in qs.Keys)
                _qs[key] = qs[key];
        }

        public String this[String key]
        {
            get { return _qs[key] ?? null; }
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