﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Linq;

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
            set { _qs[key] = value; }
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

        public string Build(Uri uri)
        {
            if (_qs.Count <= 0)
                return uri.GetLeftPart(UriPartial.Path);

            List<string> qs = new List<string>();
            foreach (var kv in _qs.Where(x => !string.IsNullOrEmpty(x.Key)))
                qs.Add(kv.Key + "=" + HttpUtility.UrlEncode(kv.Value));

            if(qs.Count > 0)
                return uri.GetLeftPart(UriPartial.Path) + "?" + string.Join("&", qs);
            else
                return uri.GetLeftPart(UriPartial.Path);
        }

        public void RedirectToSelf()
        {
            HttpContext.Current.Response.Redirect(Build(HttpContext.Current.Request.Url));
        }
    }

}