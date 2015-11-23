using Qube.Web.Core;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using Qube.Extensions;

namespace Qube.Web.UI
{
    public class Captcha : HtmlCustomControl, IRequiresSessionState, IValidator
    {
        public Captcha()
            : base("img")
        {
            Width = 200;
            Height = 50;
        }

        CustomValidator cv;
        public string ValidationGroup { get; set; }
        public string ValidationTextBoxID { get; set; }
        public string ErrorMessage { get; set; }
        public ValidatorDisplay Display { get; set; }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cv = new CustomValidator()
            {
                EnableClientScript = false,
                ValidationGroup = ValidationGroup,
                ValidateEmptyText = true
            };
            cv.ServerValidate += cv_ServerValidate;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cv.ControlToValidate = ValidationTextBoxID;
            cv.ErrorMessage = ErrorMessage;
            cv.Display = Display;
            Controls.Add(cv);
            if (!Page.IsPostBack)
                Regenerate();
        }

        void cv_ServerValidate(object source, ServerValidateEventArgs args)
        {
            TextBox txCaptcha = (TextBox)Page.FindControlRecursive(ValidationTextBoxID);

            if (HttpContext.Current.Session["Captcha_" + ID] != null && txCaptcha.Text.Trim() == HttpContext.Current.Session["Captcha_" + ID].ToString())
            {
                HttpContext.Current.Session["Valid_" + ID] = true;
                args.IsValid = true;
            }
            else
            {
                Regenerate();
                args.IsValid = false;
                txCaptcha.Text = string.Empty;

            }
        }

        public void Regenerate()
        {
            Text = GenerateRandomCode();
            HttpContext.Current.Session["Valid_" + ID] = false;
            HttpContext.Current.Session["Captcha_" + ID] = Text;
            Attributes["src"] = GenerateImage();
        }

        public string Text { get; set; }
        public Bitmap Image { get; set; }
        public new int Width { get; set; }
        public new int Height { get; set; }

        private string familyName;
        private Random random = new Random();

        private string GenerateRandomCode()
        {
            string s = "";
            for (int i = 0; i < 6; i++)
                s = string.Concat(s, this.random.Next(10).ToString());
            return s;
        }

        private void SetFamilyName(string name)
        {
            try
            {
                Font font = new Font(familyName, 12F);
                familyName = name;
                font.Dispose();
            }
            catch (Exception)
            {
                familyName = FontFamily.GenericSerif.Name;
            }
        }

        private string GenerateImage()
        {
            Image = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(Image);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width, Height);
            HatchBrush hbr = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White);
            g.FillRectangle(hbr, r);
            float fSize = r.Height + 1;
            Font f;
            SizeF size;
            do
            {
                fSize--;
                f = new Font(familyName, fSize, FontStyle.Bold);
                size = g.MeasureString(Text, f);
            } while (size.Width > r.Width);

            StringFormat format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            GraphicsPath path = new GraphicsPath();
            path.AddString(Text, f.FontFamily, (int)f.Style, f.Size, r, format);
            float v = 4F;
            PointF[] points =
            {
                new PointF(this.random.Next(r.Width) / v, this.random.Next(r.Height) / v),
                new PointF(r.Width - this.random.Next(r.Width) / v, this.random.Next(r.Height) / v),
                new PointF(this.random.Next(r.Width) / v, r.Height - this.random.Next(r.Height) / v),
                new PointF(r.Width - this.random.Next(r.Width) / v,r.Height - 
                this.random.Next(r.Height) / v)
            };

            Matrix mx = new Matrix();
            mx.Translate(0F, 0F);
            path.Warp(points, r, mx, WarpMode.Perspective, 0F);
            hbr = new HatchBrush(HatchStyle.LargeConfetti, Color.DarkGray, Color.Gray);
            g.FillPath(hbr, path);

            int m = Math.Max(r.Width, r.Height);
            for (int i = 0; i < (int)(r.Width * r.Height / 30F); i++)
            {
                int x = this.random.Next(r.Width);
                int y = this.random.Next(r.Height);
                int w = this.random.Next(m / 50);
                int h = this.random.Next(m / 50);
                g.FillEllipse(hbr, x, y, w, h);
            }

            f.Dispose();
            hbr.Dispose();
            g.Dispose();

            MemoryStream ms = new MemoryStream();
            Image.Save(ms, ImageFormat.Png);
            return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
        }


        public bool IsValid
        {
            get
            {
                return (bool)HttpContext.Current.Session["Valid_" + ID];
            }
            set
            {
                HttpContext.Current.Session["Valid_" + ID] = value;
            }
        }

        public void Validate()
        {
            cv.Validate();
        }
    }

}
