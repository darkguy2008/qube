using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Qube.Net
{
    public class WebClientEx : WebClient
    {
        public bool GZip = false;
        public readonly CookieContainer container = new CookieContainer();
        Dictionary<String, String> WebClientHeaders = new Dictionary<String, String>();

        public WebClientEx()
        {
            Init();
        }

        public void Init()
        {
            GZip = true;
            Encoding = Encoding.UTF8;
            WebClientHeaders["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            WebClientHeaders["Accept-Language"] = "en-US,en;q=0.8";
            WebClientHeaders["Accept-Encoding"] = "gzip, deflate, sdch";
            WebClientHeaders["Cache-Control"] = "no-cache";
            WebClientHeaders["Pragma"] = "no-cache";
            WebClientHeaders["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
            foreach (KeyValuePair<String, String> kv in WebClientHeaders)
                if (Headers[kv.Key] != null)
                {
                    Headers.Remove(kv.Key);
                    Headers.Add(kv.Key, kv.Value);
                }
                else
                    Headers[kv.Key] = kv.Value;
        }

        public void SetHeader(String key, String value)
        {
            if (Headers[key] != null)
            {
                Headers.Remove(key);
                Headers.Add(key, value);
            }
            else
                Headers[key] = value;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                if (GZip)
                    request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }
}
