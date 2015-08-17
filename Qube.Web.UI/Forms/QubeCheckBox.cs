using System;
using System.Web.UI.WebControls;

namespace Qube.Web.UI
{
    public class QubeCheckBox : CheckBox, IQubeFormField
    {
        // Interface members
        public String FieldName { get; set; }
        public String DataField { get; set; }

        public QubeCheckBox()
        {
        }

        protected override void Render(System.Web.UI.HtmlTextWriter w)
        {
            String text = Text;
            Text = String.Empty;

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
    }
}