﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Qube.Web.Core
{

    public class WebResource : WebControl
    {
        private HtmlCustomControl ctrl { get; set; }
        private EResourceType type { get; set; }
        public enum EResourceType
        {
            CSS = 1,
            Javascript = 2,
            Favicon = 3
        }

        public string Path { get; set; }
        public bool Cache { get; set; }

        public WebResource(EResourceType t)
        {
            Cache = true;
            type = t;

            switch (type)
            {
                case EResourceType.CSS:
                    ctrl = new HtmlCustomControl("link");
                    break;
                case EResourceType.Javascript:
                    ctrl = new HtmlCustomControl("script");
                    break;
                case EResourceType.Favicon:
                    ctrl = new HtmlCustomControl("link");
                    break;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void Render(HtmlTextWriter w)
        {
            string tick = DateTime.Now.Ticks.ToString();
            if (Path == null)
                Path = string.Empty;

            string url = Page.ResolveClientUrl(HttpUtility.HtmlDecode(Path));
            if(!Cache)
                if(url.Contains("?") || url.Contains("#"))
                    url = url + "&_t=" + tick;
                else if(!url.Contains("?"))
                    url = url + "?_t=" + tick;
                
            switch (type)
            {
                case EResourceType.CSS:
                    ctrl.Attributes.Add("rel", "stylesheet");
                    ctrl.Attributes.Add("href", url);
                    break;
                case EResourceType.Javascript:
                    ctrl.Attributes.Add("type", "text/javascript");
                    ctrl.Attributes.Add("src", url);
                    break;
                case EResourceType.Favicon:
                    ctrl.Attributes.Add("rel", "icon");
                    ctrl.Attributes.Add("href", url);
                    break;
            }

            // http://madskristensen.net/post/remove-whitespace-from-your-aspnet-page
            using (HtmlTextWriter hw = new HtmlTextWriter(new StringWriter()))
            {
                ctrl.RenderBeginTag(hw);
                ctrl.RenderEndTag(hw);
                string html = hw.InnerWriter.ToString();
                html = Regex.Replace(html, @"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}", string.Empty);
                html = Regex.Replace(html, @"[ \f\r\t\v]?([\n\xFE\xFF/{}[\];,<>*%&|^!~?:=])[\f\r\t\v]?", "$1");
                html = html.Replace(";\n", ";");
                html = HttpUtility.HtmlDecode(html);
                w.Write(html.Trim());
            }
        }
    }

    public class CSSRes : WebResource
    {
        public CSSRes() : base(EResourceType.CSS) { }
    }

    public class JSRes : WebResource
    {
        public JSRes() : base(EResourceType.Javascript) { }
    }

    public class FaviconRes : WebResource
    {
        public FaviconRes() : base(EResourceType.Favicon) { }
    }

}