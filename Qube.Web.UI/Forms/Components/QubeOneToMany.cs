using Qube.Globalization;
using Qube.Web.Core;
using System.Web.UI.WebControls;
using System;
using System.Linq;

namespace Qube.Web.UI
{
    public class QubeOneToMany : HtmlCustomControl, IQubeFormField
    {
        // Interface members
        public bool RenderLabel { get; set; }
        public string DisplayName { get; set; }
        public string DisplayFormat { get; set; }
        public string DataField { get; set; }
        public string DataFormatString { get; set; }
        public string OnClientValueChanged { get; set; }
        public string ValidationGroup { get; set; }

        public bool Required { get; set; }
        public string ValidatorPlaceHolder { get; set; }

        public string EmptyErrorMessage { get; set; }

        private CustomValidator cv;
        private GlobalizedStrings Lang;

        private Button btnAdd { get; set; }
        private Button btnDel { get; set; }
        public ListBox lbMaster { get; set; }
        public ListBox lbChosen { get; set; }
        public string MasterLabel { get; set; }
        public string ChosenLabel { get; set; }

        public QubeOneToMany() : base("table")
        {
            btnAdd = new Button() { Text = ">" };
            btnDel = new Button() { Text = "<" };
            lbMaster = new ListBox();
            lbChosen = new ListBox();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Lang = new GlobalizedStrings();

            btnAdd.ID = ID + "btnAdd";
            btnDel.ID = ID + "btnDel";
            lbMaster.ID = ID + "_lbMaster";
            lbChosen.ID = ID + "_lbChosen";

            lbMaster.Width = Width;
            lbChosen.Width = Width;
            lbMaster.Height = Height;
            lbChosen.Height = Height;
            lbMaster.SelectionMode = ListSelectionMode.Multiple;
            lbChosen.SelectionMode = ListSelectionMode.Multiple;

            btnAdd.Click += BtnAdd_Click;
            btnDel.Click += BtnDel_Click;

            HtmlCustomControl tr = new HtmlCustomControl("tr");
            HtmlCustomControl tdMaster = new HtmlCustomControl("td");
            tdMaster.Controls.Add(new HtmlCustomControl("strong", MasterLabel));
            tdMaster.Controls.Add(new HtmlCustomControl("br"));
            tdMaster.Controls.Add(lbMaster);
            HtmlCustomControl tdButtons = new HtmlCustomControl("td");
            tdButtons.Controls.Add(btnAdd);
            tdButtons.Controls.Add(new HtmlCustomControl("br"));
            tdButtons.Controls.Add(btnDel);
            HtmlCustomControl tdChosen = new HtmlCustomControl("td");
            tdChosen.Controls.Add(new HtmlCustomControl("strong", ChosenLabel));
            tdChosen.Controls.Add(new HtmlCustomControl("br"));
            tdChosen.Controls.Add(lbChosen);
            tr.Controls.Add(tdMaster);
            tr.Controls.Add(tdButtons);
            tr.Controls.Add(tdChosen);

            Controls.Add(tr);

            cv = new CustomValidator()
            {
                ControlToValidate = lbChosen.ID,
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
            EmptyErrorMessage = string.IsNullOrEmpty(EmptyErrorMessage) ? Lang["TextBoxEmptyMessage"] : EmptyErrorMessage;
            DataField = string.IsNullOrEmpty(DataField) ? DisplayName : DataField;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if(lbMaster.GetSelectedIndices().Length > 0)
            {
                ListItem[] lis = lbMaster.Items.Cast<ListItem>().Where(x => x.Selected).ToArray();
                foreach (ListItem i in lis)
                    lbMaster.Items.Remove(i);
                lbChosen.Items.AddRange(lis);
                lbChosen.ClearSelection();
                lbMaster.ClearSelection();
            }
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (lbChosen.GetSelectedIndices().Length > 0)
            {
                ListItem[] lis = lbChosen.Items.Cast<ListItem>().Where(x => x.Selected).ToArray();
                foreach (ListItem i in lis)
                    lbChosen.Items.Remove(i);
                lbMaster.Items.AddRange(lis);
                lbChosen.ClearSelection();
                lbMaster.ClearSelection();
            }
        }

        void cv_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
        }

        public void SetValue(object v)
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>()
        {
            throw new NotImplementedException();
        }

        public string GetFormattedValue()
        {
            throw new NotImplementedException();
        }
    }
}
