using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using peshitta.nl.Api;
using Peshitta.Infrastructure.Sqlite;

namespace peshitta.nl
{
  
   
    [Route("[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly DbSqlContext _db;
        private readonly ILogger<ContentController> _logger;
        //TODO create service + repository etc
        public ContentController(DbSqlContext db, ILogger<ContentController> logger)
        {
            _db = db;
            _logger = logger;
        }
        [HttpGet("GetVerse/{pTextId}")]
        public async Task<IActionResult> GetVerse(int pTextId)
        {
            try
            {
                var text = await _db.DecompressVerse(pTextId);
                return Ok(text);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failure {0}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
        [HttpGet("getversehistory/{pTextId}")]
        public async Task<IActionResult> GetVerseHistory(int pTextId)
        {
            try
            {
                var text = await _db.GetVerseHistory(pTextId);
                return Ok(text);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failure {0}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
        [HttpPost("CompareTimeStamp")]
        public async Task<IActionResult> CompareTimeStamp([FromBody] TimestampParams pars)
        {
            try
            {
                if (pars == null)
                {
                    return BadRequest("body must be given");
                }
                int parLen = pars.pTextId.Length;


                var retVal = new int[parLen];

                var timeStamps = await _db.GetVerseTimeStamps(pars.pTextId);
               
                for (int i = 0; i < parLen; i++)
                {
                    retVal[i] = timeStamps[i].CompareTo(pars.pTimeStamp[i]);
                }

                return Ok(retVal);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failure {0}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

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
        [HttpPost("UpdateVerse")]
        public async Task<IActionResult> UpdateVerse([FromBody] VerseTemp vTemp)
        {
            try {
                var didAdd = await _db.CompressVerse(vTemp.pTextId, vTemp.pTimeStamp, vTemp.pContent, vTemp.pFootNoteText, vTemp.pHeaderText);
                return NoContent() ;


            }
            catch (Exception ex)
            {
                _logger.LogError("Failure {0}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
        }

        //[HttpPost("GetSiteMap")]
        //public async Task<IActionResult> GetSiteMap(GetSiteMapRequest req)
        //{
        //    UrlSet retVal = new UrlSet();
        //    var dcd = await utils.InstanceDBAsync();

        //    var bookeditionIds = dcd.Contents.BookEditions.Values.Where(w => w.active && dcd.ActivePublications.Contains(w.publishercode));

        //    foreach (var bookId in bookeditionIds)
        //    {
        //        var chaps = await dcd.ChaptersByBookIdAsync(bookId.bookid);
        //        foreach (var ch in chaps)
        //        {
                    
        //            var contents = await dcd.LoadChaptersAsync( ch.Key.chapter, bookId.bookid);

        //            //dcd.getVersesByBookEditionEtag(new[] { p.bookEditionid }, ch.chapter, 0, out DateTime maxTs);
        //            var resp = contents.Data;
        //            retVal.AddUrl(new Url()
        //            {
        //                LastModifiedDateTime = resp.Max(m => m.timestamp),
        //                Loc = string.Format(req.FormatUrl, bookId.bookid, ch.Key.chapter)
        //            });
                    
        //        }
        //    }
        //    using (var io = (MemoryStream)retVal.ToStream())
        //    {
        //        return Ok(Encoding.UTF8.GetString(io.ToArray()));
        //    }

        //}
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