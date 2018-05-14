using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace peshitta.nl
{

    public partial class Book : System.Web.UI.Page
    {
        private new BookMaster Master
        {
            get
            {
                return (BookMaster)base.Master;
            }
        }
        int _chapter;
        bool? _decompress;
        private const string HTML_ERROR = @"<html><head><title>{1}</title><body><h1>{0}</body></h1></html>";
        
        private bool Decompress
        {
            get
            {
                if (_decompress != null)
                {
                    return (bool)_decompress;
                }
                string temp = Request.QueryString["dc"];
                _decompress = !string.IsNullOrEmpty(temp);
                return (bool)_decompress;
            }
        }
        private int _goto;
        private int GoTo
        {
            get
            {
                if (_goto == 0)
                {
                    string temp = Request.QueryString["goto"];
                    if (!string.IsNullOrEmpty(temp))
                    {
                        int.TryParse(temp, out _goto);
                    }

                }
                return _goto;
            }
        }

        private IEnumerable<int> _ch;
        protected IEnumerable<int> ch
        {
            get
            {
                if (_ch == null)
                {
                    _ch = RouteData.Values["ch"] as IEnumerable<int>;
                }
                return _ch;
            }
        }
        private IEnumerable<string> _booked;
     
     
        private int Chapter
        {
            get
            {
                if (_chapter > 0)
                {
                    return _chapter;
                }
                string temp = Request.QueryString["ch"];
                bool result = int.TryParse(temp, out _chapter);                
                return result ? _chapter : 1;
            }
        }

       
        protected override void OnLoad(EventArgs e)
        {
            var dcd = utils.InstanceDBAsync().Result;
            {
                var bookEditionId = Master.BookEditions;
                var requestedBook = bookEditionId.First();
                if (!dcd.Contents.BookEditions.ContainsKey(requestedBook))
                {
                    return;
                }
                var bookInfo = dcd.Contents.BookEditions[requestedBook];
                if (bookInfo == null)
                {
                    return;
                }
                //int maxChap = bookInfo.book.bookchapters.Max(t => t.chapter);
                bool isCrawler = utils.IsCrawler(Request);
                string mePage = System.IO.Path.GetFileName(Request.Url.AbsolutePath);
                //if (BookEditionId[0] == 0)
                //{
                //    CompressText ct = new CompressText();
                //    ct.CompressBook(BookEditionId[0]);
                //}
                DateTime maxTs = DateTime.UtcNow; ;
                if (Chapter == 0 && isCrawler)
                {
                    Response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
                    string message = "We do not process such large requests.";
                    string title = "Requested data too large";
                    Response.StatusDescription = message;
                    Response.Write(string.Format(HTML_ERROR, message, title));
                    Response.End();                    
                }
                //string eTAG = Chapter > 0 ? dcd.getVersesByBookEditionEtag(editions, Chapter, GoTo, out maxTs) :dcd.getVersesByBookEditionMaxTs(editions.First(), out maxTs);
                string eTAG = "";
               // Response.Cache.SetExpires(DateTime.Now.AddDays(30));

              
                
                addBook.ShowSuggestion = !isCrawler;
                addBook.BookEdition = bookInfo;
                Master.PageKeywords += "," + bookInfo.keywords;
                addBook.PublicationTitle = "Bijbel"; //TODO from table meaning AB = Bijbel
                
                Master.Caption = Chapter > 0 ? string.Format("{0} {1}", bookInfo.title, Chapter) : bookInfo.title;
               
                addBook.BookTitle = bookInfo.title;
                var nfo = new CultureInfo(bookInfo.langid);
                addBook.LanguageCode = nfo.TwoLetterISOLanguageName;
                addBook.IsRightToLeft = nfo.TextInfo.IsRightToLeft;
                addBook.BookEditionId = bookEditionId.First();
                addBook.StartingChapter = Chapter;         
              
               var datasource = dcd.LoadChapterAsync(Chapter, bookEditionId.First()).Result;
                maxTs = datasource.LastDateTime;
                eTAG = datasource.eTAG;
                if (utils.SetLastModified(this.Page, maxTs, eTAG))
                {
                    Response.End();
                }
                addBook.DataSource = datasource.Data;
                
                addBook.DataBind();
              
            }
        }

        
    }
}