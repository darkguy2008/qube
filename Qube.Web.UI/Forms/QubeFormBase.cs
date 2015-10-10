using Qube.Extensions;
using Qube.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    public enum EQubeFormBaseFieldType
    {
        Alpha = 1,
        Numeric,
        Alphanumeric,
        Date,
        Currency,
        DropDownList,
        Checkbox,
        File,
        Password,
        Name,
        Phone,
        Email,

        Custom
    }

    public class QubeFormBase : HtmlCustomControl
    {
        public class QubeFormBaseSubmitArguments
        {
            public Dictionary<string, IQubeFormField> Fields { get; set; }
            public bool Success { get; set; }
            public bool Cancel { get; set; }
        }
        public delegate void QubeFormBaseSubmitEventHandler(QubeFormBase sender, QubeFormBaseSubmitArguments args);
        public delegate void QubeFormBaseOperationEventHandler(QubeFormBase sender, Dictionary<string, IQubeFormField> fields);
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
            QubeExtensions.ControlFinder<QubeFormBasePanel> cf = new QubeExtensions.ControlFinder<QubeFormBasePanel>();
            cf.FindChildControlsRecursive(this, false);
            return cf.FoundControls.Where(x => x.IsCurrent).Single();
        }

        public Dictionary<string, IQubeFormField> GetFields()
        {
            Panel p = GetCurrentPanel();

            QubeExtensions.ControlFinder<IQubeFormField> cfFields = new QubeExtensions.ControlFinder<IQubeFormField>();
            cfFields.FindChildControlsRecursive(p, false);

            QubeExtensions.ControlFinder<QubeFormBaseField> cfBaseFields = new QubeExtensions.ControlFinder<QubeFormBaseField>();
            cfBaseFields.FindChildControlsRecursive(p, false);

            Dictionary<string, IQubeFormField> rv = new Dictionary<string, IQubeFormField>();
            foreach (Control c in cfFields.FoundControls)
                if(!string.IsNullOrEmpty(((IQubeFormField)c).DataField))
                    rv[((IQubeFormField)c).DataField] = c as IQubeFormField;

            foreach (Control c in cfBaseFields.FoundControls)
                if (!string.IsNullOrEmpty(((QubeFormBaseField)c).DataField))
                    rv[((QubeFormBaseField)c).DataField] = ((QubeFormBaseField)c).fld as IQubeFormField;

            return rv;
        }

        public static void FieldsToObject(object dst, Dictionary<string, IQubeFormField> fields)
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

        public static void ObjectToFields(object src, Dictionary<string, IQubeFormField> fields)
        {
            PropertyInfo[] pi = src.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
                if (fields.ContainsKey(p.Name))
                {
                    object v = p.GetValue(src, null);
                    if (v != null)
                    {
                        if (v.GetType() == typeof(DateTime))
                            if (!string.IsNullOrEmpty(fields[p.Name].DataFormatString))
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
        public event EventHandler RenderControls;

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

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (RenderControls != null)
                RenderControls.Invoke(this, null);
        }
    }
    public class QubeFormBaseField : HtmlCustomControl
    {

        public int MaxLength { get; set; }
        public bool Required { get; set; }
        public bool ReadOnly { get; set; }
        public string DataField { get; set; }
        public string DataFormat { get; set; }
        public string DisplayName { get; set; }
        public string DisplayFormat { get; set; }
        public string PlaceHolder { get; set; }
        public string OnClientValueChanged { get; set; }
        public EQubeFormBaseFieldType Type { get; set; }

        public WebControl fld;
        private HtmlCustomControl lbl = new HtmlCustomControl("label");

        public QubeFormBaseField() : base("div")
        {
            MaxLength = -1;
            Required = false;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (MaxLength == -1)
                MaxLength = 50;

            switch (Type)
            {
                case EQubeFormBaseFieldType.Alpha:
                case EQubeFormBaseFieldType.Alphanumeric:
                case EQubeFormBaseFieldType.Date:
                case EQubeFormBaseFieldType.Numeric:
                case EQubeFormBaseFieldType.Password:
                case EQubeFormBaseFieldType.Currency:
                    fld = new QubeTextBox();
                    ((QubeTextBox)fld).ValidationType = (EValidationType)Type;
                    if (Type == EQubeFormBaseFieldType.Date)
                        fld.CssClass = "date";
                    if (Type == EQubeFormBaseFieldType.Password)
                        ((QubeTextBox)fld).TextMode = TextBoxMode.Password;
                    break;
                case EQubeFormBaseFieldType.Checkbox:
                    fld = new QubeCheckBox();
                    break;
                case EQubeFormBaseFieldType.DropDownList:
                    fld = new QubeDropDownList();
                    break;
                case EQubeFormBaseFieldType.File:
                    fld = new QubeFileUpload();
                    break;
            }

            if (fld != null)
            {
                fld.ID = "frm" + ID;
                this.CopyObject(fld, new string[]
                {
                "MaxLength",
                "Required",
                "DataField",
                "DataFormat",
                "DisplayName",
                "DisplayFormat",
                "PlaceHolder",
                "OnClientValueChanged",
                "Type",
                "ToolTip"
                });

                fld.Attributes.Add("placeholder", PlaceHolder);
                if (ReadOnly)
                    fld.Attributes.Add("readonly", "readonly");

                Controls.Add(fld);
            }

            ToolTip = string.Empty;
        }

        protected override void Render(HtmlTextWriter w)
        {
            HtmlCustomControl span = new HtmlCustomControl("span");
            base.RenderBeginTag(w);
            if (DisplayName != null)
            {
                lbl.Controls.Add(new Literal() { Text = DisplayName + ":" });
                lbl.RenderControl(w);
            }
            span.RenderBeginTag(w);
            base.RenderContents(w);
            span.RenderEndTag(w);
            base.RenderEndTag(w);
        }

        public T GetValue<T>()
        {
            if (fld == null)
                return default(T);
            else
                return ((IQubeFormField)fld).GetValue<T>();
        }

        public void SetValue(object v)
        {
            if (fld != null)
                ((IQubeFormField)fld).SetValue(v);
        }
    }
}
