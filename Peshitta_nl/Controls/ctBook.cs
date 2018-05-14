using Peshitta.Data.Models;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace peshitta.nl.Controls
{
    /// <summary>
    /// Renders all chapters around a div class='bible' tag.
    /// </summary>
    public sealed class BibleBookHolder : CompositeControl
    {
        public BibleBookHolder()
            : base()
        {
            this.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;
            this.CssClass = "bible";
        }
        //public string CssClass { get; set; }
        //public bool IsRightToLeft { get; set; }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }        
    }
    /// <summary>
    /// Renders a book like 'matthew' if given a chapter and a verse, it will render only that
    /// </summary>
    public sealed class ctBook : CompositeDataBoundControl
    {
       
        // chapters container with class="bible"
        private BibleBookHolder bibleBook;
        
        public ctBook()
            : base()
        {
           // Orientation = System.Web.UI.WebControls.Orientation.Vertical;
        }

       
        /// <summary>
        /// if set, will show that specific chapter, otherwise the whole book will be shown
        /// </summary>
        public int? Chapter { get; set; }
        //private int verseCount;
        
        public BookEdition BookEdition { get; set; }
        public bool ShowSuggestion { get; set; }
        public int BookEditionId { get; set; }
       
        
        /// <summary>
        /// eg Acts
        /// </summary>
        [Localizable(true)]
        public string BookTitle { get; set; }
        [Localizable(true)]
        public string PublicationTitle { get; set; }
        public bool IsRightToLeft { get; set; }
        public string LanguageCode { get; set; }
        public int StartingChapter { get; set; }
        public int? Verse { get; set; }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {//empty            
        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {//empty
        }
        
        
        /// <summary>
        /// optional css class for a href
        /// </summary>
        [DefaultValue(""), Category("Appearance"), CssClassProperty]
        public string HyperLinkCss { get; set; }

        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {

            //var bookTitleholder = new BookTitle();
            //bookTitleholder.Title = BookTitle;
            //Controls.Add(bookTitleholder);
            
            bibleBook = new BibleBookHolder();
           // bibleBook.IsRightToLeft = IsRightToLeft;
            ControlCollection cols = bibleBook.Controls;
            Controls.Add(bibleBook);
            
            int count = 0;
            IEnumerator ie = dataSource.GetEnumerator();

            int chp = StartingChapter;
            if (chp == 0) chp++;
            var chapterVerses =new  List<AlineaText>();
            Chapter ctChapter = null;
      
            while (ie.MoveNext())
            {

                var verses = (AlineaText)ie.Current;
                if (ctChapter == null || verses.Alineaid == 1)
                {
                    if (ctChapter != null && chapterVerses.Count > 0)
                    {
                        ctChapter.DataSource = chapterVerses;                        
                        ctChapter.DataBind();
                        chapterVerses = new List<AlineaText>();
                    }

                    ctChapter = new Chapter()
                    {
                        IsRightToLeft = IsRightToLeft,
                        ShowSuggestion = this.ShowSuggestion,
                        //ctChapter.LanguageCode = LanguageCode;
                        PublicationTitle = PublicationTitle,
                        //TODO: chp must increment to real chapters
                        ChapterNo = chp,
                        BookTitle = BookTitle,
                        ChapterTitle = string.Format("Hoofdstuk {0}", chp++)
                    };
                    cols.Add(ctChapter);
                    count++;
                }                
        
                chapterVerses.Add(verses);
                
                
            }
            if (ctChapter != null)
            {
                ctChapter.DataSource = chapterVerses;
                ctChapter.DataBind();
            }
            return 1;
        }
    }
}