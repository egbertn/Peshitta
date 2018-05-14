using System.Collections;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace peshitta.nl.Controls
{
    public sealed class ChapterHolder : HtmlTable
    {
        public ChapterHolder() :
            base()
        {
            this.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;;
            
            this.Attributes["class"] = "book";
            
        }
        //public string CssClass { get; set; }
        public bool IsRightToLeft { get; set; }

        //protected override HtmlTextWriterTag TagKey
        //{
        //    get
        //    {
        //        return HtmlTextWriterTag.Div;
        //    }
        //}
        //protected override void AddAttributesToRender(HtmlTextWriter writer)
        //{
        //    base.AddAttributesToRender(writer);
        //    if (IsRightToLeft)
        //    {
        //        writer.AddAttribute(HtmlTextWriterAttribute.Dir, "rtl");
        //    }
        //}
    }
    public sealed class ChapterTitle : Control
    {
        public ChapterTitle()
            : base()
        {
            this.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;; 
        }
        public string LanguageCode { get; set; }
        public string Title { get; set; }
        
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Title, Title);
            //if (!string.IsNullOrEmpty(LanguageCode))
            //{
            //    writer.AddAttribute("lang", LanguageCode);
            //}
            writer.RenderBeginTag(HtmlTextWriterTag.H4);
            writer.WriteEncodedText(Title);
            writer.RenderEndTag();

        }
    }
    ///// <summary>
    ///// &lt;h4&gt;Hoofdstuk 1&lt;/h4&gt;&lt;div class="book"&gt;&lt;h5&gt;1&lt;/h5&gt;
    ///// </summary>
    public class Chapter : CompositeDataBoundControl
    {
        //public IList<> DataSource;
        public Chapter()
            : base()
        {
            this.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;; 
        }
        
        private ChapterHolder versesHolder;
        /// <summary>
        /// eg chapter 1
        /// </summary>
        private ChapterTitle chapterTitle;

        
        public bool IsRightToLeft { get; set; }
        public bool ShowSuggestion { get; set; }

        /// <summary>
        /// renders the title of the chapter like 'hoofdstuk 1'
        /// </summary>
        public string ChapterTitle { get; set; }

        public string PublicationTitle { get; set; }
        /// <summary>
        /// facilitates to render on each verse, the prefix of the bookname by ToolTip eg 'Matthew 10:10'
        /// </summary>
        public int ChapterNo { get; set; }
        
        /// <summary>
        /// facilitates to render on each verse, the prefix of the bookname by ToolTip eg 'Matthew 10:10'
        /// </summary>
        public string BookTitle { get; set; }
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            //nothing
        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            //nothing
        }
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            //nothing
        }

        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {
            ControlCollection cols = Controls;
            
            chapterTitle = new ChapterTitle();
            //we just add to child controls <h4>title</h4><div class='book'>...</div>
            int count = 2;   
            //count++;
            chapterTitle.Title = ChapterTitle;
           // chapterTitle.LanguageCode = this.LanguageCode;
            cols.Add(chapterTitle);

            versesHolder = new ChapterHolder()
            {
                IsRightToLeft = IsRightToLeft
            };
            cols.Add(versesHolder);
            ControlCollection cols2 = versesHolder.Controls;

            IEnumerator ie = dataSource.GetEnumerator();

            while (ie.MoveNext())
            {
                var verse = new ctVerse()
                {
                    ShowSuggestion = this.ShowSuggestion
                };
                //verse.LanguageCode = LanguageCode;
                //verse.IsRightToLeft = IsRightToLeft;
                var text = (Peshitta.Data.Models.AlineaText)ie.Current;
                verse.DataBind();
                if (!string.IsNullOrEmpty(BookTitle))
                {
                    verse.ToolTip = string.Format("{0} {1}:{2}", BookTitle, ChapterNo, text.Alineaid);
                }
                verse.PublicationTitle = this.PublicationTitle;
                //here go 2 verses, normally, the same verse but for 2 languages
                verse.DataSource = text.Texts;
               
                verse.AlineaId = text.Alineaid;
                verse.BookChapterAlineaId = text.BookChapterAlineaId;
                //verse
                cols2.Add(verse);
                verse.DataBind();
            }
            return count;
        }
    }
}