using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace peshitta.nl.Controls
{
    /// <summary>
    /// Builds (this time) a flat navigation menu
    /// </summary>
    [PartialCaching(86400, "pub;nom;booked", "", "Host")] //cache for a day
    public sealed class BookIdx : CompositeControl
    {
        public BookIdx()
            : base()
        {
            Orientation = System.Web.UI.WebControls.Orientation.Vertical;
            ItemWidth = Unit.Pixel(200);

        }
        public delegate Task needData();
        //private int _bookEditionId;
        public ChaptersPerBook chapterIndex;
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            chapterIndex = new ChaptersPerBook();


        }

        public event needData OnneedData;
        //public int BookId
        //{
        //    get;

        //    set;
        //}
        public IEnumerable<int> BookEditions
        {
            get;
          
            set;
        }
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Span;
            }
        }
        public bool NoChapters { get; set; }
        /// <summary>
        /// sets or gets a hostname like www.peshitta.nl 
        /// The menu will include that name
        /// </summary>
        public string HostName { get; set; }


        public Unit ItemWidth { get; set; }



        public string SelectedCssClass { get; set; }



        public string ChaptersAreaCss { get; set; }

        /// <summary>
        /// optional css class for a href
        /// </summary>
        public string HyperLinkCss { get; set; }

        ///// <summary>
        ///// defaults to Dutch
        ///// </summary>
        //private int LangId
        //{
        //    get
        //    {                
        //        string temp = Page.Request.QueryString["langid"];
        //        int langid;
        //        return int.TryParse(temp, out langid) ? langid : 19;
        //    }
        //}

        private bool nomenu
        {
            get
            {
                string temp = Page.Request.QueryString["nom"];
                return temp == "1";
            }
        }
        public IEnumerable<Peshitta.Data.Models.BookEdition> DataSource {  get; set; }
        [DefaultValue(Orientation.Vertical)]
        public Orientation Orientation { get; set; }

        //[IsRequired(true)]
        public string BookTitle { get; set; }

        //private HyperLink[] menuItems;
        public IEnumerable<int> DataSourceIndex { get; set; }
        protected override void CreateChildControls()
        {
            if (nomenu)
            {
                return;
            }
            OnneedData?.Invoke();
            if (!NoChapters)
            {
                chapterIndex.BookTitle = BookTitle;
                chapterIndex.LanguageCode = "nl";
                var bookid = this.DataSource.First().bookid;
                chapterIndex.DataSource = DataSourceIndex;
                chapterIndex.BookEditionId = this.BookEditions.First();
                chapterIndex.BookEdsPassThrough = this.BookEditions;
                chapterIndex.HyperLinkCss = HyperLinkCss;
                chapterIndex.ChaptersAreaCss = ChaptersAreaCss;
                chapterIndex.DataBind();
            }
            int ctCount = 0;
            if (DataSource == null) return;
            var controls = Controls;
            string hrefcss = string.IsNullOrEmpty(this.HyperLinkCss) ? null : this.HyperLinkCss;
            //var fp = DataSource.Select(s => s.publishercode).Distinct().ToList();
           // var books = DataSource.Where(p =>this.BookEditions.Contains(this. p.bookid == this.BookId).AsEnumerable();
            foreach (var itm in DataSource.Where(w => w.publishercode == "AB").OrderBy(o => o.bookOrder))
            {
                var div = new WebControl(Orientation == Orientation.Vertical ? HtmlTextWriterTag.Div : HtmlTextWriterTag.Span);
                if (!ItemWidth.IsEmpty)
                {
                    div.Width = ItemWidth;
                }

                ctCount++;
                var hl = new HyperLink();
                div.Controls.Add(hl);

                controls.Add(div);
                //menuItems[count] = hl;       
                if (itm.bookEditionid == this.BookEditions.First())
                {
                    div.CssClass = this.SelectedCssClass;
                    //div.Style["float"] = "right";
                     if (!NoChapters)
                    {
                        WebControl chapterWrap = new WebControl(HtmlTextWriterTag.Span);                    
                        chapterWrap.Controls.Add(chapterIndex);
                        //chapterWrap.Controls.Add(new WebControl(HtmlTextWriterTag.Br));
                        controls.Add(chapterWrap);
                    }

                }
                string hostName = HostName;
                var Request = Page.Request;
                string scheme = Request.IsSecureConnection ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
                //TODO
                //string FormatUrl = $"{scheme}://{hostName}/book.aspx?booked={itm.bookEditionid}&ch=1";

                string FormatUrl = (!string.IsNullOrWhiteSpace(hostName) ? $"{scheme}://{hostName}" : "") + "/book.aspx?";
                var publist = DataSource.Where(s => s.bookid == itm.bookid).Select(p => p.bookEditionid).ToList();

                int x = 0;
                for (; x < publist.Count(); x++)
                {
                    FormatUrl += string.Concat("booked=", publist[x].ToString(), "&");
                }
                FormatUrl += "ch=1";               
                hl.NavigateUrl = FormatUrl;

                //TODO:  if remote host a -> arrow must be shown that the user will be redirected.

                //TODO
                //hl.Attributes.Add("hreflang", "nl");
                if (hrefcss != null)
                {
                    hl.CssClass = hrefcss;
                }

                hl.Text = //uhm, not ok
                hl.ToolTip = itm.title;
            }            
        }

        public string IndexPub
        {
            get;
            set;
       
        }
        
    }
}
