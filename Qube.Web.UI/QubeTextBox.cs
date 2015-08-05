using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

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

    public class QubeTextBox : TextBox
    {
        private CustomValidator cv;
        public bool Required { get; set; }
        public String FieldName { get; set; }
        public String ErrorMessage { get; set; }
        public String EmptyErrorMessage { get; set; }
        public String LongErrorMessage { get; set; }
        public EValidationType ValidationType { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

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
            ErrorMessage = String.IsNullOrEmpty(ErrorMessage) ? "The field <strong>{0}</strong> is invalid" : ErrorMessage;
            EmptyErrorMessage = String.IsNullOrEmpty(EmptyErrorMessage) ? "The field <strong>{0}</strong> is required" : EmptyErrorMessage;
            LongErrorMessage = String.IsNullOrEmpty(LongErrorMessage) ? "The field <strong>{0}</strong> is too long" : LongErrorMessage;
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
                        DateTime tmp = DateTime.Now;
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
    }

}
