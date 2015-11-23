using Qube.Globalization;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Web.UI;
using Qube.Web.Core;

namespace Qube.Web.UI
{

    public enum EValidationType
    {
        Alpha = 1,
        Numeric,
        Alphanumeric,
        AllChars,
        Date,
        MultiDate,
        Time,
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

    public class QubeTextBox : TextBox, IQubeFormField
    {
        private CustomValidator cv;
        public bool Required { get; set; }
        public string ErrorMessage { get; set; }
        public string EmptyErrorMessage { get; set; }
        public string LongErrorMessage { get; set; }
        public EValidationType ValidationType { get; set; }
        public string PlaceHolder { get; set; }

        // Interface members
        public bool RenderLabel { get; set; }
        public string DisplayName { get; set; }
        public string DisplayFormat { get; set; }
        public string DataField { get; set; }
        public string DataFormatString { get; set; }
        public string OnClientValueChanged { get; set; }

        private GlobalizedStrings Lang;

        public QubeTextBox()
        {
            RenderLabel = false;
            DataFormatString = "{0}";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Lang = new GlobalizedStrings();

            if (MaxLength == 0)
                MaxLength = 50;

            cv = new CustomValidator()
            {
                ControlToValidate = ID,
                EnableClientScript = false,
                ValidationGroup = ValidationGroup,
                ValidateEmptyText = true
            };
            cv.ServerValidate += cv_ServerValidate;
            Controls.Add(cv);
        }

        protected override void OnLoad(EventArgs e)
        {            
            base.OnLoad(e);
            ErrorMessage = string.IsNullOrEmpty(ErrorMessage) ? Lang["TextBoxErrorMessage"] : ErrorMessage;
            EmptyErrorMessage = string.IsNullOrEmpty(EmptyErrorMessage) ? Lang["TextBoxEmptyMessage"] : EmptyErrorMessage;
            LongErrorMessage = string.IsNullOrEmpty(LongErrorMessage) ? Lang["TextBoxLongMessage"] : LongErrorMessage;
            DataField = string.IsNullOrEmpty(DataField) ? DisplayName : DataField;

            if (!string.IsNullOrEmpty(OnClientValueChanged))
            {
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "change_fld" + ClientID,
                    "$('#" + ClientID + "').on('keydown', function() { " + OnClientValueChanged + "(); });" +
                    "$('#" + ClientID + "').on('blur', function() { " + OnClientValueChanged + "(); });",
                    true
                );
            }

            if(ValidationType == EValidationType.Currency && !string.IsNullOrEmpty(DisplayFormat))
            {
                string opts = "";
                switch (DisplayFormat.ToLowerInvariant().Trim())
                {
                    case "vef":
                        opts = "{ aSep: '.', aDec: ',', aSign: ' Bs.F', pSign: 's'}";
                        break;
                    case "usd":
                        opts = "{ aSep: ',', aDec: '.', aSign: '$ ', pSign: 'p'}";
                        break;
                }
                if (!string.IsNullOrEmpty(DisplayFormat))
                {
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "format_fld" + ClientID,
                        string.Format("$('#{0}').autoNumeric('init', {1});", ClientID, opts),
                        true
                    );
                }
            }

            if (ValidationType == EValidationType.Date)
                CssClass = "date";
            if (ValidationType == EValidationType.MultiDate)
                CssClass = "multidate";
            if (ValidationType == EValidationType.Time)
                CssClass = "time";
        }

        void cv_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (Required && string.IsNullOrWhiteSpace(args.Value))
            {
                args.IsValid = false;
                cv.ErrorMessage = string.Format(EmptyErrorMessage, DisplayName);
                return;
            }

            if (args.Value.Length > MaxLength)
            {
                args.IsValid = false;
                cv.ErrorMessage = string.Format(LongErrorMessage, DisplayName);
                return;
            }

            if (!string.IsNullOrWhiteSpace(args.Value))
            {
                switch (ValidationType)
                {
                    case EValidationType.Alpha:
                        if (!Regex.IsMatch(args.Value, @"^[a-záéíóúñA-ZÁÉÍÓÚÑ\-'`´ ]*$"))
                            args.IsValid = false;
                        break;
                    case EValidationType.Name:
                        if (!Regex.IsMatch(args.Value, @"^[a-záéíóúñA-ZÁÉÍÓÚÑ\-'`´ ]*$"))
                            args.IsValid = false;
                        break;
                    case EValidationType.Alphanumeric:
                        if (!Regex.IsMatch(args.Value, @"^[0-9a-záéíóúñA-ZÁÉÍÓÚÑ\-'`´_,. ]*$"))
                            args.IsValid = false;
                        break;
                    case EValidationType.Numeric:
                        if (!Regex.IsMatch(args.Value, "^[0-9]*$"))
                            args.IsValid = false;
                        break;
                    case EValidationType.Phone:
                        if (!Regex.IsMatch(args.Value, "^[0-9()-. ]*$"))
                            args.IsValid = false;
                        break;
                    case EValidationType.Email:
                        if (!Regex.IsMatch(args.Value, "^\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*$"))
                            args.IsValid = false;
                        break;
                    case EValidationType.Date:
                        DateTime tmp = DateTime.MinValue;
                        if (!DateTime.TryParseExact(args.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmp))
                            args.IsValid = false;
                        break;
                    case EValidationType.Time:
                        if (!DateTime.TryParseExact(args.Value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmp))
                            args.IsValid = false;
                        break;
                    case EValidationType.Password:
                        if (!Regex.IsMatch(args.Value, @"^[a-zA-Z0-9\#\!\@\$\&\*\+\-_]*$"))
                            args.IsValid = false;
                        break;
                }
                if (!args.IsValid)
                    cv.ErrorMessage = string.Format(ErrorMessage, DisplayName);
            }
            
        }

        public T GetValue<T>()
        {
            string rv = Text;

            if (!string.IsNullOrEmpty(rv))
            {
                if (ValidationType == EValidationType.Currency && !string.IsNullOrEmpty(DisplayFormat))
                {
                    switch (DisplayFormat.ToLowerInvariant().Trim())
                    {
                        case "vef":
                            rv = rv.ToLowerInvariant().Replace(" bs.f", "").Replace(".", "").Replace(",", ".").Trim();
                            return (T)(object)Single.Parse(rv, CultureInfo.InvariantCulture);
                        case "usd":
                            rv = rv.ToLowerInvariant().Replace("$ ", "").Replace(",", "").Trim();
                            return (T)(object)Single.Parse(rv, CultureInfo.InvariantCulture);
                    }
                }
            }

            return (T)(object)rv;
        }

        public void SetValue(object v)
        {
            Text = v == null ? string.Empty : v.ToString();
        }

        public string GetFormattedValue()
        {
            switch(ValidationType)
            {
                case EValidationType.Date:
                    return DateTime.ParseExact(Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(DataFormatString);
                case EValidationType.Time:
                    return DateTime.ParseExact(Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(DataFormatString);
                default:
                    return string.Format(DataFormatString, Text);
            }
        }

        protected override void Render(HtmlTextWriter w)
        {
            if (!string.IsNullOrEmpty(PlaceHolder))
                Attributes.Add("placeholder", PlaceHolder);
            if(RenderLabel && !string.IsNullOrEmpty(DisplayName))
            {
                HtmlCustomControl lbl = new HtmlCustomControl("label");
                lbl.Controls.Add(new Literal() { Text = DisplayName + ":" });
                lbl.RenderControl(w);
                base.Render(w);
            }
            else
                base.Render(w);
        }
    }

}
