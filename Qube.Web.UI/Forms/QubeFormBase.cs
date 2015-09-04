using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Qube.Web.UI.Forms
{
    public class QubeFormBase : HtmlCustomControl
    {
        public delegate void QubeFormBaseOperationEventHandler(QubeFormBase sender, Dictionary<String, IQubeFormField> fields);
        public event QubeFormBaseOperationEventHandler FirstLoad;
        public event QubeFormBaseOperationEventHandler Submit;

        public QubeFormBase() : base("div")
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InvokeFirstLoad();
        }

        public void InvokeFirstLoad()
        {
            if (FirstLoad != null)
                FirstLoad(this, GetFields());
        }

        public Panel GetCurrentPanel()
        {
            Extensions.ControlFinder<QubeFormBasePanel> cf = new Extensions.ControlFinder<QubeFormBasePanel>();
            cf.FindChildControlsRecursive(this);
            return cf.FoundControls.Where(x => x.IsCurrent).Single();
        }

        public Dictionary<String, IQubeFormField> GetFields()
        {
            Panel p = GetCurrentPanel();

            Extensions.ControlFinder<IQubeFormField> cfFields = new Extensions.ControlFinder<IQubeFormField>();
            cfFields.FindChildControlsRecursive(p, false);

            Dictionary<String, IQubeFormField> rv = new Dictionary<String, IQubeFormField>();
            foreach (Control c in cfFields.FoundControls)
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
                    {
                        p.SetValue(dst, v, null);
                    }
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
        private QubeFormBasePanel FormParent;

        public QubeFormBasePanel() : base("div")
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FormParent = Parent as QubeFormBasePanel;
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
                    lbField.RenderControl(w);
                    c.RenderControl(w);
                    continue;
                }
                c.RenderControl(w);
            }

            base.RenderEndTag(w);

            // TODO: CreateControl functions
        }
    }
}
