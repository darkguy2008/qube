using Qube.Globalization;
using System;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using static Qube.Web.Core.Extensions;
using System.Linq;
using System.Collections.Generic;

namespace Qube.Web.UI
{
    public class QubeFileUpload : FileUpload, IQubeFormField
    {
        // Interface members
        public String FieldName { get; set; }
        public String DataField { get; set; }

        public bool Required { get; set; }
        public String ValidationGroup { get; set; }
        public String ValidatorPlaceHolder { get; set; }

        public String ErrorMessage { get; set; }
        public String EmptyErrorMessage { get; set; }
        public String ValidContentTypes { get; set; }

        private CustomValidator cv;
        private GlobalizedStrings Lang;

        public QubeFileUpload()
        {
            ValidatorPlaceHolder = "QubeValidatorPlaceHolder";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Lang = new GlobalizedStrings(HttpContext.Current.Server.MapPath("~/App_GlobalResources/Qube." + CultureInfo.CurrentUICulture + ".txt"));
        }

        protected override void OnLoad(EventArgs e)
        {
            EmptyErrorMessage = String.IsNullOrEmpty(EmptyErrorMessage) ? Lang["FileUploadEmptyMessage"] : EmptyErrorMessage;
            ErrorMessage = String.IsNullOrEmpty(ErrorMessage) ? Lang["FileUploadErrorMessage"] : ErrorMessage;

            cv = new QubeCustomValidator()
            {
                ControlToValidateUniqueID = UniqueID,
                EnableClientScript = false,
                ValidationGroup = ValidationGroup,
                Display = ValidatorDisplay.None,
                ValidateEmptyText = true
            };
            if (!String.IsNullOrEmpty(EmptyErrorMessage))
                cv.ErrorMessage = String.Format(EmptyErrorMessage, FieldName);

            cv.ServerValidate += new ServerValidateEventHandler(cv_ServerValidate);

            if (!String.IsNullOrEmpty(ValidatorPlaceHolder) && Page.FindControl(ValidatorPlaceHolder) != null)
                Page.FindControl(ValidatorPlaceHolder).Controls.Add(cv);
            else if (!String.IsNullOrEmpty(ValidatorPlaceHolder) && Page.Master != null && Page.Master.FindControl(ValidatorPlaceHolder) != null)
                Page.Master.FindControl(ValidatorPlaceHolder).Controls.Add(cv);
            else
            {
                MasterPage m = Page.Master;
                ControlFinder<PlaceHolder> cf = new ControlFinder<PlaceHolder>();
                while (m.Master != null)
                    m = m.Master;
                cf.FindChildControlsRecursive(m);
                PlaceHolder vph = cf.FoundControls.Where(x => x.ID == ValidatorPlaceHolder).SingleOrDefault();
                if(vph == null)
                    throw new Exception("Cannot find asp:PlaceHolder inside page with ID '" + ValidatorPlaceHolder + "' required for validating a QubeFileUpload control");
                vph.Controls.Add(cv);
            }

            base.OnLoad(e);
        }

        private void cv_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (Required && !HasFile)
            {
                args.IsValid = false;
                cv.ErrorMessage = String.Format(EmptyErrorMessage, FieldName);
                return;
            }

            if (HasFile)
            {
                if (!String.IsNullOrEmpty(ValidContentTypes))
                {
                    List<String> allowedTypes = ValidContentTypes.Split(',').ToList();
                    for (int i = 0; i < allowedTypes.Count; i++)
                        allowedTypes[i] = allowedTypes[i].ToLowerInvariant().Trim();
                    if (!allowedTypes.Contains(PostedFile.ContentType.ToLowerInvariant().Trim()))
                    {
                        args.IsValid = false;
                        cv.ErrorMessage = String.Format(ErrorMessage, FieldName);
                        return;
                    }
                }
            }
        }

        public T GetValue<T>()
        {
            return (T)(object)FileName;
        }

        public void SetValue(object v)
        {
            //throw new NotImplementedException("Can a FileUpload filename be set?");
        }
    }
}
