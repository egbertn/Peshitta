using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Text.RegularExpressions;


namespace peshitta.nl
{
    public partial class verse : System.Web.UI.Page
    {

        protected override void OnLoad(EventArgs e)
        {
            if (utils.IsCrawler(Request))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Response.StatusDescription = "Content no longer available for crawling";
                Response.Buffer = false;
                Response.End();
            }
            if (!IsPostBack)
            {
                //Response.Cache.SetExpires(DateTime.Now.AddDays(60));
                //var bd = new Peshitta.Data.DB.KitabDB();
                //{

                //    DateTime maxTs;
                //    string eTag;
                //    //TODO: create this into a scrolling gridview

                //    var textIds = bd.getTextIdsByBookIdsAndAlineas(((popup)Master).BookEditions, ((popup)Master).GoTos, out maxTs, out eTag);
                //    if (utils.SetLastModified(Page, maxTs, eTag))
                //    {
                //        Response.End();
                //    }

                //    _txt.Text = bd.DecompressVerse(textIds.First()).Content;
                //    _versindication.Text = bd.bookTitle(textIds.First());
                //}
                _btn.OnClientClick = "opener.closeSuggest(); return false;";
            }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

    }
}