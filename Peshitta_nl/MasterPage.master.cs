using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Threading.Tasks;

namespace peshitta.nl
{
    public partial class MasterPage : System.Web.UI.MasterPage
    {
        private DateTime flTime;
        /// <summary>
        /// sets the browser caption, the title of the main HTML document.
        /// </summary>
        public string Caption
        {
            set
            {
                string app = isPeshittaNl ? string.Format(Resource._3, Resource._1) : Resource._2;
                this.Page.Title = string.Format("{0} - {1}", app, value);
            }
            get
            {
                return Page.Title;
            }
        }

        public string PageKeywords
        {
            get
            {
                return keywords.Content;
            }
            set
            {
                keywords.Content = value;
            }
        }

        //public void setLinkNextPrev(string nextLink, string nextDescription, string prevLink, string PrevDescription, string start, string startDescription)
        //{
        //    if (!string.IsNullOrEmpty(nextLink))
        //    {                
        //        HtmlLink lnk= new HtmlLink();
        //        lnk.Href = nextLink;
        //        var attr = lnk.Attributes;
        //        attr["rel"] = "next";
        //        attr["type"] = "text/html";
        //        attr["title"] = nextDescription;
        //        Header.Controls.Add(lnk);
        //    }
        //    if (!string.IsNullOrEmpty(prevLink))
        //    {
        //        HtmlLink lnk = new HtmlLink();
        //        lnk.Href = prevLink;
        //        var attr = lnk.Attributes;
        //        attr["rel"] = "prev";
        //        attr["type"] = "text/html";
        //        attr["title"] = PrevDescription;
        //        Header.Controls.Add(lnk);
        //    }

        //    if (!string.IsNullOrEmpty(start))
        //    {
        //        HtmlLink lnk = new HtmlLink();
        //        lnk.Href = start;
        //        var attr = lnk.Attributes;
        //        attr["rel"] = "start";
        //        attr["type"] = "text/html";
        //        attr["title"] = startDescription;
        //        Header.Controls.Add(lnk);
        //    }

        //}
        public bool AlreadyDateSet { private get; set; }
        private string GetHeader
        {
            get
            {
                return Request.Url.Host;
            }
        }
        public bool isLocalHost
        {
            get
            {
                string h = GetHeader;
                return h.Equals("localhost", StringComparison.OrdinalIgnoreCase) || h.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);
            }
        }
        public bool isPeshittaNl
        {
            get
            {
                return GetHeader.EndsWith("peshitta.nl");
            }
        }
        //public void Redirect(string location)
        //{
        //    Response.RedirectLocation = location;
        //    Response.StatusCode = (int)HttpStatusCode.Moved;
        //    Response.StatusDescription = "Moved Permanently";
        //    Response.Cache.SetCacheability(HttpCacheability.Public);
        //    Response.Buffer = false;
        //    Response.End();
        //}

        protected bool nomenu
        {
            get
            { return Request.QueryString["nom"] == "1"; }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this._menu != null)
            {
                this._menu.OnneedData += _menu_OnneedData;
            }
        }
        private string DefaultIndexPub
        {
            get
            {
                Control hField = (HiddenField)this.FindControl("IndexPub");
                return hField != null && hField is HiddenField ?
                    ((HiddenField)hField).Value : null;
            }
        }
        public IEnumerable<int> BookEditionId
        {
            get
            {
                string sTemp = Request.QueryString["booked"];
                if (!string.IsNullOrEmpty(sTemp))
                {
                    try
                    {
                        return sTemp.Split(',').Select(int.Parse);
                    }
                    catch
                    {
                    }

                }
                return new int[] { 1 };
            }
        }
        async Task _menu_OnneedData()
        {
            var bd = await utils.InstanceDBAsync();
            {

                var lst = (await bd.BookEditions).Data.Where(w => bd.ActivePublications.Contains(w.publishercode)).ToArray();
                var bookId = lst.FirstOrDefault(f => f.bookid == this.BookEditionId.First()).bookid;
                _menu.DataSource = bd.Contents.BookEditions.Values.Where(w => lst.Select(s => s.bookid).Contains(w.bookid));
                _menu.IndexPub = this.DefaultIndexPub;
                _menu.NoChapters = true;
                _menu.BookEditions = this.BookEditionId;
                _menu.DataBind();
                //first is considered leading and default
                //TODO: think about better algorithm
                _menu.DataSourceIndex = (await bd.ChaptersByBookIdAsync(bookId)).Select(s => s.Key.chapter).ToArray();
            }

        }

        private IEnumerable<string> PubId
        {
            get
            {
                string t = Request.QueryString["pubid"];
                return string.IsNullOrEmpty(t) ? new string[] { "AB" }
                    : t.Split(',');
            }
        }

        public IEnumerable<string> DefaultPublications
        {
            get
            {
                Control hField = (HiddenField)this.FindControl("DefaultPubs");
                if (hField != null && hField is HiddenField)
                {
                    string tmp = ((HiddenField)hField).Value;
                    return tmp.Split(',');
                }
                return new string[] { };
            }
        }
        /// <summary>
        /// defaults to Dutch
        /// </summary>
        private int LangId
        {
            get
            {
                string temp = Request.QueryString["langid"];

                return int.TryParse(temp, out int langid) ? langid : 19;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            bool isPshi = isPeshittaNl;
            if (!nomenu)
            {
                AltContent.Visible = false;
            }


            if (isPshi && Request.Path.Contains("default.aspx"))
            {

                Response.Redirect("book.aspx?booked=1&booked=27&ch=1", true);
                return;
            }
            //HtmlMeta meta = new HtmlMeta();
            // meta.Name="google-site-verification";
            //2001translation!

            // because of caching
            if (_menu != null)
            {
                _menu.HostName = isLocalHost ? Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":"+ Request.Url.Port.ToString()): "www.peshitta.nl";
                if (Request.Url.AbsolutePath.StartsWith("peshitta.nl"))
                {
                    _menu.HostName += "/peshitta.nl";
                }
            }
            links.NavigateUrl = "~/links.aspx?nom=1";
            links.Text = "Links";
            // meta.Content = "-jOpEs91q4iFMBYITAYsjQoCSChCgwtWR6N3zfuaJDw";
            ///<meta name="google-site-verification" content="-jOpEs91q4iFMBYITAYsjQoCSChCgwtWR6N3zfuaJDw" />

            Response.Headers.Add("msvalidate.01", "AEBA54FB3587D23EFBFDB31BCC1F1293");
            //  Header.Controls.Add(meta);
            Header.Controls.Add(
                new HtmlMeta()
                {
                    Name = "description",
                    Content = isPshi ? Resource.PeshittaDescr : Resource.AboutVertaling
                });
            keywords.Content = isPshi ? Resource._4 : Resource._5;

            pnlMenu.Visible = !isPeshittaNl;

            string curFile = Request.PhysicalPath; //, curFile);

            Page.ClientScript.RegisterClientScriptInclude("osb", "osb.js");


            if (!AlreadyDateSet && string.Compare(Path.GetFileNameWithoutExtension(curFile), "book", true) != 0)
            {
                flTime = new FileInfo(curFile).LastWriteTimeUtc;
                string calcHash = curFile + flTime.ToBinary().ToString();
                if (utils.SetLastModified((this.Page), flTime, calcHash.GetHashCode().ToString()))
                {
                    Response.End();
                }
            }

        }
    }
}