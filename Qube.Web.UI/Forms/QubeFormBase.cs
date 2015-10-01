﻿using Qube.Web.Core;
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

        Custom
    }

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

            Extensions.ControlFinder<QubeFormBaseField> cfBaseFields = new Extensions.ControlFinder<QubeFormBaseField>();
            cfBaseFields.FindChildControlsRecursive(p, false);

            Dictionary<String, IQubeFormField> rv = new Dictionary<String, IQubeFormField>();
            foreach (Control c in cfFields.FoundControls)
                if(!String.IsNullOrEmpty(((IQubeFormField)c).DataField))
                    rv[((IQubeFormField)c).DataField] = c as IQubeFormField;

            foreach (Control c in cfBaseFields.FoundControls)
                if (!String.IsNullOrEmpty(((QubeFormBaseField)c).DataField))
                    rv[((QubeFormBaseField)c).DataField] = ((QubeFormBaseField)c).fld as IQubeFormField;

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
        private QubeTextBox _tx;
        private QubeCheckBox _cb;
        private QubeFileUpload _file;
        private QubeDropDownList _ddl;
        private HtmlCustomControl _label;
        public IQubeFormField fld = null;

        public int MaxLength { get; set; }
        public bool Required { get; set; }
        public String FieldName { get; set; }
        public String DataField { get; set; }
        public String PlaceHolder { get; set; }
        public String DisplayFormat { get; set; }
        public String ValueFormat { get; set; }
        public bool ReadOnly { get; set; }
        public String OnClientValueChanged { get; set; }
        public EQubeFormBaseFieldType Type { get; set; }

        public QubeFormBaseField() : base("div")
        {            
            this.Init += QubeFormBaseField_Init;
            this.Load += QubeFormBaseField_Load;
            MaxLength = -1;
            Required = false;
            DisplayFormat = String.Empty;
            ValueFormat = String.Empty;
        }

        private void QubeFormBaseField_Init(object sender, EventArgs e)
        {
            _tx = new QubeTextBox();
            _cb = new QubeCheckBox();
            _file = new QubeFileUpload();
            _ddl = new QubeDropDownList();
            _label = new HtmlCustomControl("label");
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
                    fld = _tx;
                    ((QubeTextBox)fld).ReadOnly = ReadOnly;
                    break;
                case EQubeFormBaseFieldType.Checkbox:
                    fld = _cb;
                    ((QubeCheckBox)fld).Enabled = !ReadOnly;
                    break;
                case EQubeFormBaseFieldType.DropDownList:
                    fld = _ddl;
                    ((QubeDropDownList)fld).Enabled = !ReadOnly;
                    break;
                case EQubeFormBaseFieldType.File:
                    fld = _file;
                    ((QubeFileUpload)fld).Enabled = !ReadOnly;
                    break;
            }
        }

        private void QubeFormBaseField_Load(object sender, EventArgs e)
        {
            if (Type != EQubeFormBaseFieldType.Custom)
            {
                PropertyInfo[] piSrc = this.GetType().GetProperties();
                PropertyInfo[] piDst = fld.GetType().GetProperties();
                foreach (PropertyInfo pd in piDst)
                    foreach (PropertyInfo ps in piSrc)
                        if (ps.Name == pd.Name)
                            if (pd.CanWrite)
                                pd.SetValue(fld, ps.GetValue(this, null), null);
                ToolTip = String.Empty;

                ((Control)fld).ID = "frm" + ((Control)fld).ID;
                ((WebControl)fld).Attributes.Add("placeholder", PlaceHolder);
                switch (Type)
                {
                    case EQubeFormBaseFieldType.Date:
                        ((WebControl)fld).CssClass = "date";
                        break;
                    case EQubeFormBaseFieldType.Password:
                        ((TextBox)fld).TextMode = TextBoxMode.Password;
                        break;
                }

                Controls.Add((Control)fld);

                if (Type != EQubeFormBaseFieldType.Custom)
                {
                    switch (Type)
                    {
                        case EQubeFormBaseFieldType.Currency:
                            String opts = "";
                            switch (DisplayFormat.ToLowerInvariant().Trim())
                            {
                                case "vef":
                                    opts = "{ aSep: '.', aDec: ',', aSign: ' Bs.F', pSign: 's'}";
                                    break;
                                case "usd":
                                    opts = "{ aSep: ',', aDec: '.', aSign: '$ ', pSign: 'p'}";
                                    break;
                            }
                            if (!String.IsNullOrEmpty(DisplayFormat))
                            {
                                Page.ClientScript.RegisterStartupScript(Page.GetType(), "format_fld" + ((WebControl)fld).ClientID,
                                    String.Format("$('#{0}').autoNumeric('init', {1});", ((WebControl)fld).ClientID, opts),
                                    true
                                );
                            }
                            break;
                    }

                    switch (Type)
                    {
                        case EQubeFormBaseFieldType.Alpha:
                        case EQubeFormBaseFieldType.Alphanumeric:
                        case EQubeFormBaseFieldType.Date:
                        case EQubeFormBaseFieldType.Numeric:
                        case EQubeFormBaseFieldType.Password:
                        case EQubeFormBaseFieldType.Currency:
                            if(!String.IsNullOrEmpty(OnClientValueChanged))
                            {
                                Page.ClientScript.RegisterStartupScript(Page.GetType(), "change_fld" + ((WebControl)fld).ClientID,
                                    "$('#" + ((WebControl)fld).ClientID + "').on('keydown', function() { PageMethods." + OnClientValueChanged + "(); });",
                                    true
                                );
                            }
                            break;
                    }

                }
            }
        }

        protected override void Render(HtmlTextWriter w)
        {
            HtmlCustomControl span = new HtmlCustomControl("span");
            base.RenderBeginTag(w);
            if (FieldName != null)
            {
                _label.Controls.Add(new Literal() { Text = FieldName + ":" });
                _label.RenderControl(w);
            }
            span.RenderBeginTag(w);
            base.RenderContents(w);
            span.RenderEndTag(w);
            base.RenderEndTag(w);
        }

        public void SetValue(object v)
        {
            switch (Type)
            {
                case EQubeFormBaseFieldType.Alpha:
                case EQubeFormBaseFieldType.Alphanumeric:
                case EQubeFormBaseFieldType.Date:
                case EQubeFormBaseFieldType.Numeric:
                case EQubeFormBaseFieldType.Password:
                case EQubeFormBaseFieldType.Currency:
                    ((QubeTextBox)fld).Text = v.ToString();
                    break;
                case EQubeFormBaseFieldType.Checkbox:
                    ((QubeCheckBox)fld).Checked = (bool)v;
                    break;
                case EQubeFormBaseFieldType.DropDownList:
                    ((QubeDropDownList)fld).SelectedValue = v.ToString();
                    break;
            }
        }
    }
}
