using System;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace peshitta.nl
{
    public partial class Contact : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            btnContact.Click += btnContact_Click;            
            ValidPhone.ServerValidate += ValidPhone_ServerValidate;
        }
        protected override void OnLoad(EventArgs e)
        {            
            if (!IsPostBack)
            {
                FullName.Focus();
            }
        }
        private void btnContact_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (Page.IsValid)
            {
                msgLine.Style["display"] = "none";
                lblMessage.Visible = false;
                string body = string.Format("email from: {0}\r\nSubject {1}\r\nPhone: {2}\r\nMail: {3}",
                    FullName.Text, Subject.Text, Phone.Text, Email.Text);

                utils.LogByMail("Neem contact met mij op", body);
                lblMessage.Visible = true;


            }
            else
            {
                msgLine.Style["display"] = "block";
                lblMessage.Visible = false;
            }

        }
        private void ValidPhone_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string value = args.Value;
            for (int cx = 0; cx < value.Length; cx++)
            {
                if (!char.IsDigit(value[cx]))
                {
                    if (value[cx] != '-')
                    {
                        args.IsValid = false;
                        return;
                    }
                }
            }
            args.IsValid = true;
        }
    }

}