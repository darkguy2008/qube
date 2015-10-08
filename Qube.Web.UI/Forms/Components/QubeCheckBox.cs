using Qube.Web.Core;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    public class QubeCheckBox : CheckBox, IQubeFormField
    {
        // Interface members
        public bool RenderLabel { get; set; }
        public string DisplayName { get; set; }
        public string DisplayFormat { get; set; }
        public string DataField { get; set; }
        public string DataFormatString { get; set; }
        public string OnClientValueChanged { get; set; }

        public QubeCheckBox()
        {
            RenderLabel = false;
        }

        protected override void Render(System.Web.UI.HtmlTextWriter w)
        {
            string text = Text;
            Text = string.Empty;

            if (RenderLabel && !string.IsNullOrEmpty(DisplayName))
            {
                HtmlCustomControl lbl = new HtmlCustomControl("label");
                lbl.Controls.Add(new Literal() { Text = DisplayName + ":" });
                lbl.RenderControl(w);
            }

            if (TextAlign == TextAlign.Left)
            {
                Label l = new Label() { Text = text };
                l.RenderControl(w);
                base.Render(w);
            }
            else
            {
                base.Render(w);
                Label l = new Label() { Text = text };
                l.RenderControl(w);
            }
        }

        public T GetValue<T>()
        {
            return (T)(object)Checked;
        }

        public void SetValue(object v)
        {
            Checked = (bool)v;
        }

        public string GetFormattedValue()
        {
            return string.Format(Checked.ToString(), DataFormatString);
        }

    }
}