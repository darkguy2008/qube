using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    public class QubeFormBase : HtmlCustomControl
    {
        public class QubeFormBaseSubmitArguments
        {
            public Dictionary<String, IQubeFormField> Fields { get; set; }
            public bool Success { get; set; }
            public bool Cancel { get; set; }
        }
        public delegate void QubeFormBaseSubmitEventHandler(QubeFormBase sender, QubeFormBaseSubmitArguments args);
        public delegate void QubeFormBaseOperationEventHandler(QubeFormBase sender, Dictionary<String, IQubeFormField> fields);
        public event EventHandler FormLoad;
        public event QubeFormBaseOperationEventHandler FirstLoad;
        public event QubeFormBaseSubmitEventHandler Submit;

        public QubeFormBase() : base("div")
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InvokeLoad(e);
            InvokeFirstLoad();
        }

        public void InvokeLoad(EventArgs e)
        {
            if (FormLoad != null)
                FormLoad(this, e);
        }

        public void InvokeFirstLoad()
        {
            if (FirstLoad != null)
                FirstLoad(this, GetFields());
        }

        public void InvokeSubmit(QubeFormBaseSubmitArguments args)
        {
            if (Submit != null)
                Submit.Invoke(this, args);
        }

        public Panel GetCurrentPanel()
        {
            Extensions.ControlFinder<QubeFormBasePanel> cf = new Extensions.ControlFinder<QubeFormBasePanel>();
            cf.FindChildControlsRecursive(this, false);
            return cf.FoundControls.Where(x => x.IsCurrent).Single();
        }

        public Dictionary<String, IQubeFormField> GetFields()
        {
            Panel p = GetCurrentPanel();

            Extensions.ControlFinder<IQubeFormField> cfFields = new Extensions.ControlFinder<IQubeFormField>();
            cfFields.FindChildControlsRecursive(p, false);

            Dictionary<String, IQubeFormField> rv = new Dictionary<String, IQubeFormField>();
            foreach (Control c in cfFields.FoundControls)
                if(!String.IsNullOrEmpty(((IQubeFormField)c).DataField))
                    rv[((IQubeFormField)c).DataField] = c as IQubeFormField;

            return rv;
        }

        public static void FieldsToObject(object dst, Dictionary<String, IQubeFormField> fields)
        {
            PropertyInfo[] pi = dst.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
                if (fields.ContainsKey(p.Name))
                {
                    object v = Convert.ChangeType(fields[p.Name].GetValue<object>(), p.PropertyType);
                    if (p.CanWrite)
                        p.SetValue(dst, v, null);
                }
        }

        public static void ObjectToFields(object src, Dictionary<String, IQubeFormField> fields)
        {
            PropertyInfo[] pi = src.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
                if (fields.ContainsKey(p.Name))
                {
                    object v = p.GetValue(src, null);
                    if (v != null)
                    {
                        if (v.GetType() == typeof(DateTime))
                            if (!String.IsNullOrEmpty(fields[p.Name].DataFormatString))
                                v = ((DateTime)v).ToString(fields[p.Name].DataFormatString);
                    }
                    fields[p.Name].SetValue(v);
                }
        }
    }

    public class QubeFormBasePanel : HtmlCustomControl
    {
        public bool IsCurrent { get; set; }
        protected QubeFormBase FormParent;

        public event EventHandler LoadControls;

        public QubeFormBasePanel() : base("div")
        {
            this.Init += QubeFormBasePanel_Init;
        }

        private void QubeFormBasePanel_Init(object sender, EventArgs e)
        {
            FormParent = Parent as QubeFormBase;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (LoadControls != null)
                LoadControls.Invoke(this, null);
        }

        protected override void Render(HtmlTextWriter w)
        {
            base.RenderBeginTag(w);

            foreach (Control c in Controls)
            {
                if (c.GetType().GetInterfaces().Contains(typeof(IQubeFormField)))
                {
                    HtmlCustomControl lbField = new HtmlCustomControl("label");
                    lbField.Attributes["for"] = c.ClientID;
                    lbField.Controls.Add(new Literal() { Text = ((IQubeFormField)c).FieldName + ":" });
                    HtmlCustomControl span = new HtmlCustomControl("span");
                    lbField.RenderControl(w);
                    span.RenderBeginTag(w);
                    c.RenderControl(w);
                    span.RenderEndTag(w);
                    continue;
                }
                else if (c.GetType() == typeof(Captcha))
                {
                    HtmlCustomControl lbField = new HtmlCustomControl("label");
                    lbField.Controls.Add(new Literal() { Text = "&nbsp;" });
                    HtmlCustomControl span = new HtmlCustomControl("span");
                    lbField.RenderControl(w);
                    span.RenderBeginTag(w);
                    c.RenderControl(w);
                    span.RenderEndTag(w);
                }
                else
                    c.RenderControl(w);
            }

            base.RenderEndTag(w);
        }
    }
}
