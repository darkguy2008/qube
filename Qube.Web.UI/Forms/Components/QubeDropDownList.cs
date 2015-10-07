using Qube.Globalization;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Qube.Web.Core;
using Qube.Extensions;

namespace Qube.Web.UI
{
    public class QubeDropDownList : DropDownList, IQubeFormField
    {
        // Interface members
        public bool RenderLabel { get; set; }
        public string DisplayName { get; set; }
        public string DisplayFormat { get; set; }
        public string DataField { get; set; }
        public string DataFormatString { get; set; }
        public string OnClientValueChanged { get; set; }

        public bool Required { get; set; }
        public string ValidatorPlaceHolder { get; set; }

        public string EmptyErrorMessage { get; set; }

        private CustomValidator cv;
        private GlobalizedStrings Lang;

        public QubeDropDownList()
        {
            RenderLabel = false;
            ValidatorPlaceHolder = "QubeValidatorPlaceHolder";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Lang = new GlobalizedStrings();
        }

        protected override void OnLoad(EventArgs e)
        {
            EmptyErrorMessage = string.IsNullOrEmpty(EmptyErrorMessage) ? Lang["DropDownListEmptyMessage"] : EmptyErrorMessage;

            cv = new QubeCustomValidator()
            {
                ControlToValidateUniqueID = UniqueID,
                EnableClientScript = false,
                ValidationGroup = ValidationGroup,
                Display = ValidatorDisplay.None,
                ValidateEmptyText = true
            };
            if (!string.IsNullOrEmpty(EmptyErrorMessage))
                cv.ErrorMessage = string.Format(EmptyErrorMessage, DisplayName);

            cv.ServerValidate += new ServerValidateEventHandler(cv_ServerValidate);

            if (!string.IsNullOrEmpty(ValidatorPlaceHolder) && Page.FindControl(ValidatorPlaceHolder) != null)
                Page.FindControl(ValidatorPlaceHolder).Controls.Add(cv);
            else if (!string.IsNullOrEmpty(ValidatorPlaceHolder) && Page.Master != null && Page.Master.FindControl(ValidatorPlaceHolder) != null)
                Page.Master.FindControl(ValidatorPlaceHolder).Controls.Add(cv);
            else
            {
                MasterPage m = Page.Master;
                QubeExtensions.ControlFinder<PlaceHolder> cf = new QubeExtensions.ControlFinder<PlaceHolder>();
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
            if (Required && (SelectedItem == null || string.IsNullOrEmpty(SelectedItem.Value)))
            {
                args.IsValid = false;
                cv.ErrorMessage = string.Format(EmptyErrorMessage, DisplayName);
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
            return string.Format(SelectedItem.Value, DataFormatString);
        }

        protected override void Render(HtmlTextWriter w)
        {
            if (RenderLabel && !string.IsNullOrEmpty(DisplayName))
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