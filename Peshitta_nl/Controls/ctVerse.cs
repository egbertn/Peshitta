using System;
using System.Web.UI;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Diagnostics;

namespace peshitta.nl.Controls
{
    //public sealed class SeoTag : CompositeControl //composite causes <span>
    //{
    //    private IList<word> _words;
    //    public SeoTag(IList<word> words)
    //        : base()
    //    {

    //        ViewStateMode = System.Web.UI.ViewStateMode.Disabled;;
    //        _words = words;
    //        //this.ClientIDMode = System.Web.UI.ClientIDMode.AutoID;
    //        this.CssClass = "seo_2";
    //    }
    //    public void EnsureControls()
    //    {
    //        this.EnsureChildControls();
    //    }
    //    protected override void CreateChildControls()
    //    {
    //        //space must be between the () tags. e.g. ( Alaha) , not (Alaha).            
    //        Controls.Add(new PlainText(string.Format(" ({0})", string.Join(", ", _words.Select(t => t.word1)))));

    //    }
    //    public override void RenderControl(HtmlTextWriter writer)
    //    {
         
    //        this.Render(writer);
    //        writer.Flush();
    //    }
    //}


    public sealed class Note : IComparable
    {
        private readonly string _key;
        private readonly string _value;
        private readonly int _pos;

        public Note(string key, string value, int pos)
        {
            _key = key;
            _value = value;
            _pos = pos;
        }
        /// <summary>
        /// Since we want the first found keyword first we hash it on position
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Position;
        }
        public string Key { get { return _key; } }
        public string Value { get { return _value; } }
        /// <summary>
        /// returns position of the key within the plain Content (text)
        /// </summary>
        public int Position { get { return _pos; } }

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            var compNote = (Note)obj;
            if (compNote.Position < Position) return 1;
            if (compNote.Position == Position) return 0;
            return -1;
        }

        #endregion
    }

    /// <summary>
    /// plaintext holder, holds the verse and eventually, breaks down into hyperlinks
    /// </summary>
    public sealed class PlainTextHolder : HtmlTableCell
    {
        private List<Note> _notes;
        private IList<ParagraphNote> _pnotes;
        internal PlainTextHolder()
        //: base(HtmlTextWriterTag.Div)
        {
            this.Direction = ContentDirection.NotSet;
            this.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;;
            this.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _notes = new List<Note>();
            _pnotes = new List<ParagraphNote>();
        }
        public IList<Note> notes
        {
            get
            {
                return _notes;
            }
        }
        public ContentDirection Direction { get; set; }
        public bool ShowSuggestion { get; set; }
        public int VerseNumber
        {
            get
            {
                EnsureChildControls();
                int b = Direction == ContentDirection.LeftToRight ? 0 : 1;
                return ((NumSpan)Controls[0]).VerseNumber;

            }
            set
            {
                EnsureChildControls();
                int b = Direction == ContentDirection.LeftToRight ? 0 : 1;
                ((NumSpan)Controls[0]).VerseNumber = value;

            }

        }

        public void AddTitle(string title)
        {
        }
        public int TextId { private get; set; }

        public override string ClientID
        {
            get
            {
                return ID;
            }
        }
        public override string ID
        {
            get
            {
                return string.Format("t{0}", TextId);
            }
        }
        public string CssClass { get; set; }

        protected override void CreateChildControls()
        {
            this.Attributes["dir"] = Direction == ContentDirection.LeftToRight ? "ltr" : "rtl";
            this.Attributes["class"] = this.CssClass;

            Controls.Add(new NumSpan());
            Controls.Add(new PlainText());

        }
        public string Content { set; private get; }

        public IList<ParagraphNote> AddFootNote(string footNoteText)
        {
            string[] notes = !string.IsNullOrEmpty(footNoteText) ? footNoteText.Split(new string[] { "\n", "<br/>", "<br>" }, StringSplitOptions.RemoveEmptyEntries) : new string[0];

            EnsureChildControls();
            int notesLen = string.IsNullOrEmpty(footNoteText) ? 0 : notes.Length;

            _notes = new List<Note>(notesLen);

            for (int y = 0; y < notesLen; y++)
            {
                int findSplits = notes[y].IndexOf(':');
                if (findSplits > 0)
                {
                    //sort by index, first keyword found, comes first!

                    findSplits = notes[y].IndexOf(':');

                    string key = notes[y].Substring(0, findSplits);
                    int pos = Content.IndexOf(key, 0, StringComparison.Ordinal);
                    if (pos >= 0)
                    {
                        _notes.Add(new Note(key, notes[y].Substring(findSplits + 1).TrimStart(' '), pos));
                    }
                }
                //TODO: fix
                //else
                //{
                //    throw new HttpException(string.Format("Footnote found {0} but no : keyword separator used index = {1}", t.Remarks, t.textid));
                //}
            }
            bool avoidFirstPlainText = false;
            //sort using key (default because of GetHashCode)
            _notes.Sort();
            notesLen = _notes.Count;

            ////breaking down to smaller parts
            int contLen = Content.Length;
            int keywordpos = 0;
            int prevPos = 0;

            for (int x = 0; ; x++)
            {
                if (x < notesLen)
                {
                    keywordpos = _notes[x].Position;
                }
                else if (x == notesLen)
                {
                    if (prevPos < contLen)
                    {
                        Controls.Add(new PlainText());
                    }
                    //end the loop
                    break;
                }
                int keywlen = _notes[x].Key.Length;
                prevPos = keywordpos + keywlen;
                if (avoidFirstPlainText == false)
                {
                    avoidFirstPlainText = true;
                }
                else
                {
                    if (prevPos < contLen)
                    {
                        Controls.Add(new PlainText());
                    }
                }
                Controls.Add(new FootNoteLink());
            }
            for (int x = 0; x < notesLen; x++)
            {
                _pnotes.Add(new ParagraphNote());
            }
            return _pnotes;
        }


        protected override void OnPreRender(EventArgs e)
        {

            if (Direction == ContentDirection.RightToLeft)
            {
                ((NumSpan)Controls[0]).Style[HtmlTextWriterStyle.FontSize] = "70%";
            }
            if (_notes.Count == 0)
            {
                //just a single text
                ((PlainText)Controls[1]).Text = this.Content;
            }
            else
            {
                int controlCount = 1;
                int keywordpos = 0;
                int prevPos = 0;
                //TODO: case sensitive


                int contLen = Content.Length;
                int x = 0;
                int notesLen = _notes.Count;
                for (x = 0; ; x++)
                {
                    if (x == notesLen || keywordpos < 0)
                    {
                        if (prevPos < contLen)
                        {
                            //take remainer
                            ((PlainText)Controls[controlCount++]).Text = Content.Substring(prevPos);
                        }
                        break;
                    }
                    keywordpos = _notes[x].Position;
                    var divPos = keywordpos - prevPos;
                    if (keywordpos >= prevPos ) //includes a note at the very first letter of a row
                    {
                        ((PlainText)Controls[controlCount++]).Text = Content.Substring(prevPos, divPos);
                    }
                    if (Controls[controlCount] is FootNoteLink)
                    {
                        var btn = (FootNoteLink)Controls[controlCount++];
                        btn.Text = _notes[x].Key;
                        btn.OnClick = string.Format("FixNote(event,{0});", x);
                        btn.OnMouseOver = string.Format("ShowNote(event,{0});", x);
                        btn.OnMouseOut = string.Format("HideNote(event,{0});", x);
                    }
                    prevPos = keywordpos + _notes[x].Key.Length;
                }
                for (x = 0; x < notesLen; x++)
                {
                    _pnotes[x].Text = _notes[x].Value;
                }

            }
        }
    }
    /// <summary>
    /// renders &lt;p class='noteText'&gt;text&lt;/p&gt;
    /// </summary>
    public sealed class ParagraphNote : WebControl, ITextControl
    {
        internal ParagraphNote()
            : base(HtmlTextWriterTag.Div)
        {
            base.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;;

            this.CssClass = "noteText";

        }
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
        }
        /// <summary>
        /// sets text of the paragraph
        /// </summary>
        public string Text { get; set; }
        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.Write(Text);
        }
    }
    /// <summary>
    /// renders a clickable or mouseover link on a word or part of a sentence
    /// it uses the a tag
    /// </summary>
    //[ControlValueProperty("Text")]
    public sealed class FootNoteLink : WebControl, ITextControl
    {
        public FootNoteLink()
            : base(HtmlTextWriterTag.A)
        {
            this.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;;
            this.CssClass = "note";


        }
        // [UrlProperty]
        public string NavigateUrl { get; set; }


        /// <summary>
        /// sets the text of the hyperlink
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// sets the onclick control attribute
        /// </summary>
        public string OnClick { get; set; }
        public string OnMouseOver { get; set; }
        public string OnMouseOut { get; set; }
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            if (!string.IsNullOrEmpty(OnClick))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, OnClick);
            }
            if (!string.IsNullOrEmpty(OnMouseOver))
            {
                writer.AddAttribute("onmouseover", OnMouseOver);
            }
            if (!string.IsNullOrEmpty(OnMouseOut))
            {
                writer.AddAttribute("onmouseout", OnMouseOut);
            }
            if (!string.IsNullOrEmpty(NavigateUrl))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, NavigateUrl, true);
            }
        }
        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.Write(Text);
        }
    }


    //#region versindicator
    //public sealed class VerseIndicator : Control
    //{
    //    internal VerseIndicator()
    //        : base()
    //    {
    //        this.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;;
    //        // this.CssClass = "ind";
    //    }
    //    internal int Verse { get; set; }
    //    internal string CssClass { get; set; }
    //    protected override void Render(HtmlTextWriter writer)
    //    {

    //        //if (!string.IsNullOrEmpty(CssClass))
    //        //{
    //        //    writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
    //        //}
    //        //writer.RenderBeginTag(HtmlTextWriterTag.Label);
    //        writer.Write(Verse);
    //        writer.Write(' ');//space
    //        //writer.RenderEndTag();

    //    }
    //}
    //#endregion

    public sealed class SeoTag1 : HtmlTableCell, ITextControl
    {
        HyperLink h;
        public SeoTag1()
            : base()
        {
            this.Attributes["class"] = "seo_1";
        }

        protected override void CreateChildControls()
        {
            h = new HyperLink();
            h.CssClass = "seo_1";
            Controls.Add(h);
        }

        public string Text
        {
            get
            {
                EnsureChildControls();
                return h.Text;
            }
            set
            {
                EnsureChildControls();
                h.Text = value;
            }
        }
    }
    internal sealed class NumSpan : WebControl
    {
        internal NumSpan()
            : base(HtmlTextWriterTag.Span)
        {
            //this.Attributes["class"] = "num";
            this.CssClass = "num";

        }
        public int VerseNumber { get; set; }


        protected override void RenderContents(HtmlTextWriter writer)
        {
            //writer.Write(' ');
            writer.Write(VerseNumber);
            writer.Write(' ');

        }
    }

    #region TextHeader
    /// <summary>
    /// renders a simple header that can be used as a title above any verse.
    /// no HTML encoding is performed. (This can be risky if author is non HTML aware)
    /// </summary>
    sealed class VerseTitle : WebControl
    {
        internal VerseTitle(string pText)
            : base(HtmlTextWriterTag.Div)
        {
            base.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;;
            this.CssClass = "texthead";
            this.Text = pText;
        }
        internal string Text { set; get; }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.Write(Text);
        }
    }
    #endregion
    #region plaintext
    /// <summary>
    /// contains text without any html rendering, symbols or footnotes.
    /// Note that if the text contains things like &lt;span or &lt;b&gt; it 
    /// will be rendered 'as is' no HTML checking or encoding is done in order
    /// to keep the wishes of the writer in tact.
    /// </summary>
    public sealed class PlainText : Control, ITextControl
    {

        public PlainText()
            : base()
        {
            base.ViewStateMode = ViewStateMode.Disabled;
        }

        public PlainText(string p)
        {
            // TODO: Complete member initialization
            this.Text = p;
        }
        /// <summary>
        /// just the literal text dude
        /// </summary>
        public string Text { set; get; }


        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(Text);
        }
    }
    #endregion

    ///// <summary>
    ///// renders the Title of the bible book shown at the top.
    ///// </summary>
    //sealed class BookTitle : Control
    //{
    //    internal BookTitle()
    //        : base()
    //    {
    //    }
    //    public string Title { get; set; }

    //    public string CssClass { get; set; }
    //    protected override void Render(HtmlTextWriter writer)
    //    {
    //        if (!string.IsNullOrWhiteSpace(Title))
    //        {
    //            if (!string.IsNullOrEmpty(CssClass))
    //            {
    //                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
    //            }
    //            writer.RenderBeginTag(HtmlTextWriterTag.H2);
    //            writer.Write(Title);
    //            writer.RenderEndTag();
    //        }
    //    }
    //}


    /*
     * <p id="p219" <!-- this is the ptag-->>  <span class="syc" lang="syc" style="width="25%">ܟ݁ܰܕ݂ ܕ݁ܶܝܢ ܐܶܬ݂ܺܝܠܶܕ݂ ܝܶܫܽܘܥ ܒ݁ܒ݂ܶܝܬ݂‌ܠܚܶܡ ܕ݁ܺܝܗܽܘܕ݂ܳܐ ܒ݁ܝܰܘܡܰܝ ܗܶܪܳܘܕ݂ܶܣ ܡܰܠܟ݁ܳܐ ܐܶܬ݂ܰܘ ܡܓ݂ܽܘܫܶܐ ܡܶܢ ܡܰܕ݂ܢܚܳܐ ܠܽܐܘܪܺܫܠܶܡ </span> 
     *                          <span class="syc" lang="syc" style="width="25%">ܟ݁ܰܕ݂ ܕ݁ܶܝܢ ܐܶܬ݂ܺܝܠܶܕ݂ ܝܶܫܽܘܥ ܒ݁ܒ݂ܶܝܬ݂‌ܠܚܶܡ ܕ݁ܺܝܗܽܘܕ݂ܳܐ ܒ݁ܝܰܘܡܰܝ ܗܶܪܳܘܕ݂ܶܣ ܡܰܠܟ݁ܳܐ ܐܶܬ݂ܰܘ ܡܓ݂ܽܘܫܶܐ ܡܶܢ ܡܰܕ݂ܢܚܳܐ ܠܽܐܘܪܺܫܠܶܡ </span>
     * <a class="seo_1">Bijbel Handelingen 2:1</a><span class="num"> 1 </span><span class="verse" lang="nl">
     * Nadat de dagen van Pentiqosti<span class="seo_2"> (pinksteren)</span> waren vervuld, waren allen samen 
     * vergaderd.</span></p>
     * */
    /*<p id="p222">
     * <a class="seo_1">Bijbel Handelingen 2:30</a>
     * <span class="num"> 30 </span>
     * <span class="verse" lang="nl">
     * Want hij was een profeet, en hij wist dat Alaha<span class="seo_2"> (god)</span> 
     * hem een eed had gezworen: <b>"<a onclick="FixNote(event,0);" 
     * onmouseover="ShowNote(event,0);" 
     * onmouseout="HideNote(event,0);" class="note">
     * Ik zal</a> [iemand] uit de vrucht van uw schoot op uw troon doen zitten."</b>
     * <a title="Suggestie geven!" href="javascript:suggest(6505);"> #</a></span>
     * </p><!-- end of ptag><p class="noteText">Psalmen 89:4; 132:11</p>

     * */
    // een <span class="verse">(PlainTextHolder) kan uit meerdere PlainText controls bestaan!
    public class VersesRow : HtmlTableRow
    {
        public VersesRow()
            : base()
        {
            base.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;;
            //_rtlText = new List<Text>();
            //_ltrText = new List<Text>();
            this.ClientIDMode = System.Web.UI.ClientIDMode.Static;

        }


        //public override string ID { get; set; }
        public string CssClass { get; set; }

        public string ToolTip { get; set; }
        public string PublicationTitle { get; set; }

        private SeoTag1 _seo;


        /// <summary>
        /// if true, will prefix a verse with a verse number
        /// </summary>
        public bool ShowVerseIndicator { get; set; }
        public int AlineaId { get; set; }
        public int VerseNumber { get; set; }
        private int TotalRtlControls;
        private int TotalLtrControls;
        public void AddPlainTextHolder(PlainTextHolder holder)
        {

            EnsureChildControls();

            if (holder.Direction == ContentDirection.RightToLeft)
            {
                Controls.AddAt(0, holder); //before the seo tag
                TotalRtlControls++;
            }
            else
            {
                Controls.Add(holder);//ad the end of the list
                TotalLtrControls++;
            }
        }

        protected override void CreateChildControls()
        {
            _seo = new SeoTag1();
            Cells.Add(_seo);
        }


        protected override void OnPreRender(EventArgs e)
        {

            _seo.Text = string.Concat(PublicationTitle, " ", this.ToolTip);
            //this.Style[HtmlTextWriterStyle.VerticalAlign] = "top";           

            double w = 95 / (TotalLtrControls + TotalRtlControls);
            foreach (PlainTextHolder hdr in Controls.OfType<PlainTextHolder>())
            {
                hdr.VerseNumber = AlineaId;
                if (TotalLtrControls + TotalRtlControls > 1)
                {
                    hdr.Style[HtmlTextWriterStyle.Width] = Unit.Percentage(w).ToString();
                }
            }


        }
    }

    internal sealed class Suggestion : HyperLink
    {
        internal Suggestion(int pTextid)
            : base()
        {
            this.textId = pTextid;
        }
        private int textId;
        protected override void Render(HtmlTextWriter writer)
        {
            this.NavigateUrl = string.Format("javascript:suggest({0});", textId);
            this.ToolTip = "Suggestie geven!";
            this.Text = " #";
            base.Render(writer);
        }
    }

    /// <summary>
    /// Renders one bible verse and one or more translations/langauges at a row
    /// </summary>
    public sealed class ctVerse : VersesRow
    {
        private IEnumerable<Peshitta.Data.Models.TextExpanded> _texts;

        public ctVerse()
            : base()
        {
            ShowVerseNumber = true;
        }

        public int BookChapterAlineaId { get; set; }
        public override string ClientID
        {
            get
            {
                return ID;
            }
        }
        public override string ID
        {
            get
            {
                return string.Format("ba{0}", this.BookChapterAlineaId);
            }
        }

        public bool ShowVerseNumber { get; set; }
        public IEnumerable<Peshitta.Data.Models.TextExpanded> DataSource
        {
            get { return _texts; }
            set { _texts = value; }

        }
        public override void DataBind()
        {
            ChildControlsCreated = false;
            //Controls.Clear();
            EnsureChildControls();
        }

        /*
         * 
         * <p id="100">Hij zei: "Toon 
           <a class="note" onclick="ShowHideNote(event,0);">berouw</a> 
           [want] het koninkrijk van de hemel is nabij"
           </p>
           <p class='noteText'> t’shuvah omkeren, terugkeren; “omkeren van de zonde en terugkeren naar de Schepper”.</p>
         * <h5>3</h5>

         * <div><h5>21</h5>
         * <p id="42">
         *  <a class="note" onclick="ShowHideNote(event,1);">
         *  Zij zal
         *  </a> 
         *  geboorte geven aan een zoon en zij zal hem Jezus noemen, want hij zal 
         *  <a class="note" onclick="ShowHideNote(event,0);">zijn mensen
         *  </a> 
         *  van hun zonden bevrijden."
         *  </p>
         *  <p class='noteText'> Aramees (15814) Leameh kan 'mens, natie, heidenen' betekenen. Vanwege meervoud 'hun zonden' gekozen voor mensen, grieks Laos.</p>
         *  <p class='noteText'> In het Aramees zowel 'jij zult' als 'zij zal' betekenen. In overeenstemming met Lukas 1:31
         *  </p>
         *  </div>
         * */
        protected override void CreateChildControls()
        {
            if (_texts == null)
            {
                return;
            }
            base.CreateChildControls();
            //SMELL: should be gotten from some other source which covers both languages
            //var fHEader = _texts.FirstOrDefault(f => !string.IsNullOrEmpty(f.Header));
            //if (fHEader != null)
            //{
            //    VerseTitle vt = new VerseTitle(fHEader.Header);
            //    Controls.Add(vt);
            //}

            foreach (var t in _texts)
            {
                //the actual verse in specific bookedition comes here
                var pnl = new PlainTextHolder()
                {
                    Content = t.Content,

                    TextId = t.TextId
                };
               
                if (t.langid == 90)
                {
                    pnl.CssClass = "syc";

                    pnl.Attributes["lang"] = "syc";
                    pnl.Direction = ContentDirection.RightToLeft;
                    pnl.ShowSuggestion = false;
                }
                else
                {
                    pnl.CssClass = "verse";
                    pnl.Direction = ContentDirection.LeftToRight;
                    pnl.ShowSuggestion = ShowSuggestion;
                }
                Trace.TraceInformation("written RtlControls textholder textid={0} alina {1} lang={2}", t.TextId, t.Alineaid, t.langid);
                AddPlainTextHolder(pnl);
                if (!string.IsNullOrEmpty(t.Remarks))
                {
                    var pNotes = pnl.AddFootNote(t.Remarks);
                    
                    foreach(var p in pNotes)
                        pnl.Controls.Add(p);
                }
            }
        }
        public bool ShowSuggestion { get; set; }
    }
}