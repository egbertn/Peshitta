using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using System.Text;
using System.Linq;
namespace peshitta.nl.Controls
{
    /// <summary>
    /// Summary description for ctChaptersPerBook
    /// </summary>
    public sealed class ChaptersPerBook : Control
    {
        public ChaptersPerBook()
            : base()
        {
        }
        public int BookEditionId { get; set; }
        /// <summary>
        /// on each chapter link, a book title 'Matthew 7' will be shown
        /// </summary>
        public string BookTitle { get; set; }
        /// <summary>
        /// optional css class for a href
        /// </summary>
        [DefaultValue(""), Category("Appearance"), CssClassProperty]
        public string HyperLinkCss { get; set; }
        
        public string LanguageCode { get; set; }
        public IEnumerable<int> DataSource { get; set; }

        
        string curPage;
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            curPage = System.IO.Path.GetFileName(this.Page.Request.Url.AbsolutePath);
        }
        
        [DefaultValue(""), Category("Appearance"), CssClassProperty]
        public string ChaptersAreaCss { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (DataSource == null || ! DataSource.Any())
            {
                return;
            }
            if (!string.IsNullOrEmpty(ChaptersAreaCss))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, ChaptersAreaCss);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
           
            int chapterCount = DataSource.Count();
            string csshref = string.IsNullOrEmpty(HyperLinkCss) ? null : HyperLinkCss;
            IEnumerable<int> data = DataSource;
            //string page = this.TemplateSourceDirectory;
            bool titleExists = !string.IsNullOrEmpty(BookTitle);
            bool langCodeExists = !string.IsNullOrEmpty(LanguageCode);
            StringBuilder FormatUrl = new StringBuilder();
           foreach(int chapter in data)
            {                
                FormatUrl.Clear();
                FormatUrl.Append ( curPage + "?");
                foreach (var e in this.BookEdsPassThrough)
                {
                    FormatUrl.AppendFormat("booked={0}&", e);
                }
                FormatUrl.AppendFormat("ch={0}", chapter);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, FormatUrl.ToString());
                if (titleExists)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title,string.Format("{0} {1}", BookTitle, chapter));
                }
                if (langCodeExists)
                {
                    writer.AddAttribute("lang", LanguageCode);
                }
                if (csshref != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, csshref);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);                
                writer.Write(BookTitle);
                writer.Write(' ');
                writer.RenderEndTag();
                writer.Write(chapter);
                writer.RenderEndTag();
                writer.Write(' ');
            }
            writer.RenderEndTag();//p
        }


        public IEnumerable<int> BookEdsPassThrough { get; set; }
    }
}