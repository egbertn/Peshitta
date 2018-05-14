using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace peshitta.nl
{
    public partial class popup : System.Web.UI.MasterPage
    {
        protected override void OnLoad(EventArgs e)
        {

        }
        public IEnumerable<int> BookEditions
        {
            get
            {
                string temp = Request.QueryString["bookid"];
                if (!string.IsNullOrEmpty(temp))
                {
                    return temp.Split(',').Select(int.Parse);
                }
                else
                {
                    return null;
                }
            }
        }
        public IEnumerable<int> GoTos
        {
            get
            {
                string temp = Request.QueryString["goto"];
                if (!string.IsNullOrEmpty(temp))
                {
                    return temp.Split(',').Select(int.Parse);
                }
                else
                {
                    return null;
                }
            }
        }

    }

}