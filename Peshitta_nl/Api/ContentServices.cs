using adccure;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace peshitta.nl
{
    public class TimestampParams
    {
        public int[] pTextId;
        public DateTime[] pTimeStamp;
    }
    public class VerseTemp
    {
        public int pTextId;
        public DateTime pTimeStamp;
        public string pContent;
        public string pFootNoteText;
        public string pHeaderText;
    }
    public class GetSiteMapRequest
    {
        public string[] pubcode;
        public int langid;
        public string FormatUrl;
    }
    [RoutePrefix("api/ContentServices")]
    public class ContentController : ApiController
    {
        [Route("GetVerse/{pTextId}"), HttpGet]
        public async Task<IHttpActionResult> GetVerse(int pTextId)
        {

            var dcd = await utils.InstanceDBAsync();
            
            return Ok(await dcd.DecompressVerse(pTextId));
            
         
        }
        [Route("CompareTimeStamp"), HttpPost]
        public async Task<IHttpActionResult> CompareTimeStamp([FromBody] TimestampParams pars)
        {
            if (pars == null)
            {
                return BadRequest("pars cannot be null");
            }
            int parLen = pars.pTextId.Length;


            var retVal = new int[parLen];
            var dcd = await utils.InstanceDBAsync();
            {

                for (int i = 0; i < parLen; i++)
                {
                    retVal[i] = dcd.CompareTimeStamp(pars.pTextId[i], pars.pTimeStamp[i]);
                }
            }
            return Ok(retVal);

        }

        //translation.Text[] IContentServices.Search(string searchString, string[] translations, int langId)
        //{
        //    using (var dcd = new BookDal())
        //    {
        //        var words = dcd.CompressTerm(searchString);
        //        //TODO AB AND 19!!!
        //        var bookEds = new List<bookedition>();

        //        bookEds.AddRange(    dcd.BookEditionsByPublishCode("AB"));

        //        var verseIds = dcd.FindRange(words, bookEds, true, 1000);
        //        var txt = new List<Text>();
        //        foreach (int verseId in verseIds)
        //        {
        //            var t = dcd.DecompressVerse(verseId);
        //            txt.Add(t);
        //        }
        //        return txt.ToArray();

        //    }
        //}
        [Route("UpdateVerse"), HttpPost]
        public async Task<IHttpActionResult> UpdateVerse([FromBody] VerseTemp vTemp)
        {
            try {

                var dcd = await utils.InstanceDBAsync();

                var textResponse = await dcd.DecompressVerse(vTemp.pTextId);
                if (textResponse.Data == null || !textResponse.Data.Any())
                {
                    throw new InvalidOperationException("pTextId not found");
                }
                var text = textResponse.Data.First().Texts.First();
                if (text != null)
                {

                    text.timestamp = vTemp.pTimeStamp;
                    text.Header = vTemp.pHeaderText;
                    text.Remarks = vTemp.pFootNoteText;
                    text.Content = vTemp.pContent;
                   
                     var didAdd = await dcd.CompressVerse(text);

                    //dcd.RollBack();


                    //TODO fix do afterwards! (otherwise lots of performance get lost)
                   
                    return Ok(true);
                }
                return StatusCode(System.Net.HttpStatusCode.NoContent);


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost, Route("GetSiteMap")]
        public async Task<IHttpActionResult> GetSiteMap(GetSiteMapRequest req)
        {
            UrlSet retVal = new UrlSet();
            var dcd = await utils.InstanceDBAsync();

            var bookeditionIds = dcd.Contents.BookEditions.Values.Where(w => w.active && dcd.ActivePublications.Contains(w.publishercode));

            foreach (var bookId in bookeditionIds)
            {
                var chaps = await dcd.ChaptersByBookIdAsync(bookId.bookid);
                foreach (var ch in chaps)
                {
                    
                    var contents = await dcd.LoadChaptersAsync( ch.Key.chapter, bookId.bookid);

                    //dcd.getVersesByBookEditionEtag(new[] { p.bookEditionid }, ch.chapter, 0, out DateTime maxTs);
                    var resp = contents.Data;
                    retVal.AddUrl(new Url()
                    {
                        LastModifiedDateTime = resp.Max(m => m.timestamp),
                        Loc = string.Format(req.FormatUrl, bookId.bookid, ch.Key.chapter)
                    });
                    
                }
            }
            using (var io = (MemoryStream)retVal.ToStream())
            {
                return Ok(Encoding.UTF8.GetString(io.ToArray()));
            }

        }
        //void IContentServices.EnsureSeoTag(string original, string[] seoTag)
        //{
        //    using (var dec = new BookDal())
        //    {
        //        dec.EnsureSeoTag(original, seoTag);
        //    }
        //}

        //string[] IContentServices.SeoTags(string original)
        //{
        //    using (var dec = new BookDal())
        //    {
        //        return dec.GetTags(original);
        //    }
        //}
 
    }
}