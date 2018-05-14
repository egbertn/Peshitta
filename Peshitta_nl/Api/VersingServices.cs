using System.Web.Http;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Peshitta.Data.DB;
using Peshitta.Data.Models;

namespace peshitta.nl.Api
{
    [RoutePrefix("api/VersingServices")]
    public class VersingController : ApiController
    {
        [Route("GetBookEditionsFromPub/{pubid}"), HttpGet]
        public async Task<IHttpActionResult> GetBookEditionsFromPub(string pubId)
        {
            var _bd = await utils.InstanceDBAsync();
            
            var list = (await _bd.BookEditions).Data.Where(p => p.publishercode == pubId);

            return Ok( list.Select(lst => lst.bookEditionid).ToArray());
            
             
        }
        [Route("GetBookInfos"), HttpPost]
        public async Task<IHttpActionResult> GetBookInfos(IEnumerable<int> bookeditionIds)
        {
            var _bd = await utils.InstanceDBAsync();
            {
                //TODO: replace with COL.
                var lst = (await _bd.BookEditions).Data.Where(b=> bookeditionIds.Contains(b.bookEditionid)).OrderBy(o => o.bookOrder).Select(c => new BookEdition()
                {
                    bookEditionid = c.bookEditionid,
                    EnglishTitle = c.EnglishTitle,
                    title = c.title,
                    isbn = c.isbn
                });

                return Ok(lst.ToArray());
            }
                        
        }

        [Route("GetVersToolTip/{textid}/{langid}"), HttpGet]
        public async Task<IHttpActionResult> GetVersToolTip(int textid, int langid)
        {
            /*
           * PARAMETERS textid Long;
   SELECT [chapter] & ":" & text.Alineaid AS vs, book.Title AS book, bookedition.langid, bookedition.publishercode, bookedition.Year, bookchapter.chapter, Text.Alineaid
   FROM bookedition INNER JOIN (book INNER JOIN (bookchapter INNER JOIN (bookchapteralinea INNER JOIN [Text] ON (bookchapteralinea.bookchapteralineaid = Text.BookChapterAlineaid) AND (bookchapteralinea.Alineaid = Text.Alineaid)) ON bookchapter.bookchapterid = bookchapteralinea.bookchapterid) ON book.bookid = bookchapter.bookid) ON bookedition.bookEditionid = Text.bookeditionid
   WHERE (((Text.textid)=[textid]));

           * */
            var _bd = await utils.InstanceDBAsync();


            foreach (var pubs in _bd.ActivePublications)
            {
                if (_bd.Contents.Pubs[pubs].Texts.ContainsKey(textid))
                {
                    var text = _bd.Contents.Pubs[pubs].Texts[textid];
                    var bca = _bd.Contents.BookChapterAlineas[new Peshitta.Data.Models.BookChapterAlineaKey(text.BookChapterAlineaid, text.Alineaid)];
                    var bc = _bd.Contents.BookChapters[bca.BookchapterId];
                    // var booked = _bd.Contents.BookEditions[text.bookeditionid];
                    var booked = _bd.Contents.BookEditions[text.bookeditionid];

                    var retVal = new 
                    {
                        Book = booked.title,
                        Chapter = bc.chapter,
                        TextId = textid,
                        Verse = text.Alineaid,
                        BookEnglish = booked.EnglishTitle
                    };

                    return Ok(retVal);
                }
            }
            return NotFound();

        }
        [Route("GetEnglishTitleFromBookEdition/{bookeditionId}"), HttpGet]
        public async Task<IHttpActionResult> GetEnglishTitleFromBookEdition(int bookeditionId)
        {

            var _bd = await utils.InstanceDBAsync();
            {
                return Ok((await _bd.BookEditions).Data.FirstOrDefault(w => w.bookEditionid== bookeditionId).EnglishTitle);
            }
        }
    }
}