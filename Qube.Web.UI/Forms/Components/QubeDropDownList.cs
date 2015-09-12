using Qube.Globalization;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Qube.Web.Core;

namespace Qube.Web.UI
{
    public class QubeDropDownList : DropDownList, IQubeFormField
    {
        // Interface members
        public String FieldName { get; set; }
        public String DataField { get; set; }
        public String DataFormatString { get; set; }

        public bool Required { get; set; }
        public String ValidatorPlaceHolder { get; set; }

        public String EmptyErrorMessage { get; set; }

        private CustomValidator cv;
        private GlobalizedStrings Lang;

        public QubeDropDownList()
        {
            ValidatorPlaceHolder = "QubeValidatorPlaceHolder";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Lang = new GlobalizedStrings();
        }

        protected override void OnLoad(EventArgs e)
        {
            EmptyErrorMessage = String.IsNullOrEmpty(EmptyErrorMessage) ? Lang["DropDownListEmptyMessage"] : EmptyErrorMessage;

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
                Extensions.ControlFinder<PlaceHolder> cf = new Extensions.ControlFinder<PlaceHolder>();
                while (m.Master != null)
                    m = m.Master;
                cf.FindChildControlsRecursive(m);
                PlaceHolder vph = cf.FoundControls.Where(x => x.ID == ValidatorPlaceHolder).SingleOrDefault();
                if (vph == null)
                    throw new Exception("Cannot find asp:PlaceHolder inside page with ID '" + ValidatorPlaceHolder + "' required for validating a QubeDropdownList control");
                vph.Controls.Add(cv);
            }

            base.OnLoad(e);
        }

        private void cv_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (Required && (SelectedItem == null || String.IsNullOrEmpty(SelectedItem.Value)))
            {
                args.IsValid = false;
                cv.ErrorMessage = String.Format(EmptyErrorMessage, FieldName);
                return;
            }
        }

        public T GetValue<T>()
        {
            return (T)(object)SelectedItem.Value;
        }

        public void SetValue(object v)
        {
            SelectedValue = v.ToString();
        }

        public string GetFormattedValue()
        {
            return String.Format(SelectedItem.Value, DataFormatString);
        }
    }
}