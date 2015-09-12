using Qube.Globalization;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Qube.Web.UI
{

    public enum EValidationType
    {
        Alpha = 1,
        Numeric,
        Alphanumeric,
        Name,
        Email,
        Date,
        Password,
        Phone
    }

    public class QubeTextBox : TextBox, IQubeFormField
    {
        private CustomValidator cv;
        public bool Required { get; set; }
        public String ErrorMessage { get; set; }
        public String EmptyErrorMessage { get; set; }
        public String LongErrorMessage { get; set; }
        public EValidationType ValidationType { get; set; }

        // Interface members
        public String FieldName { get; set; }
        public String DataField { get; set; }
        public String DataFormatString { get; set; }

        private GlobalizedStrings Lang;

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
            ErrorMessage = String.IsNullOrEmpty(ErrorMessage) ? Lang["TextBoxErrorMessage"] : ErrorMessage;
            EmptyErrorMessage = String.IsNullOrEmpty(EmptyErrorMessage) ? Lang["TextBoxEmptyMessage"] : EmptyErrorMessage;
            LongErrorMessage = String.IsNullOrEmpty(LongErrorMessage) ? Lang["TextBoxLongMessage"] : LongErrorMessage;
            DataField = String.IsNullOrEmpty(DataField) ? FieldName : DataField;
        }

        void cv_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (Required && String.IsNullOrWhiteSpace(args.Value))
            {
                args.IsValid = false;
                cv.ErrorMessage = String.Format(EmptyErrorMessage, FieldName);
                return;
            }

            if (args.Value.Length > MaxLength)
            {
                args.IsValid = false;
                cv.ErrorMessage = String.Format(LongErrorMessage, FieldName);
                return;
            }

            if (!String.IsNullOrWhiteSpace(args.Value))
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
                    case EValidationType.Password:
                        if (!Regex.IsMatch(args.Value, @"^[a-zA-Z0-9\#\!\@\$\&\*\+\-_]{8,15}$"))
                            args.IsValid = false;
                        break;
                }
                if (!args.IsValid)
                    cv.ErrorMessage = String.Format(ErrorMessage, FieldName);
            }
            
        }

        public T GetValue<T>()
        {
            return (T)(object)Text;
        }

        public void SetValue(object v)
        {
            Text = v == null ? String.Empty : v.ToString();
        }

        public string GetFormattedValue()
        {
            switch(ValidationType)
            {
                case EValidationType.Date:
                    return DateTime.ParseExact(Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(DataFormatString);
                default:
                    return String.Format(Text, DataFormatString);
            }
        }
    }

}
