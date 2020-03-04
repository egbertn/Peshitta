
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Peshitta.Infrastructure.Models;
using Peshitta.Infrastructure.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace peshitta.nl.Api
{
    [Route("[controller]")]
    public class VersingController : ControllerBase
    {
        //TODO 
        private readonly DbSqlContext _db;
        private readonly ILogger<VersingController> _logger;
        public VersingController(DbSqlContext db, ILogger<VersingController> logger)
        {
            _db = db;
            _logger = logger;
        }
        [HttpGet("GetBookEditionsFromPub/{pubid}")]
        public async Task<IActionResult> GetBookEditionsFromPub(string pubId)
        {
            try
            {
                var list = await _db.BookEdition.Where(p => p.publishercode == pubId).
                    Select(s => s.bookEditionid).ToArrayAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failure {0}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
        [HttpPost("GetBookInfos")]
        public async Task<IActionResult> GetBookInfos([FromBody] IEnumerable<int> bookeditionIds)
        {
            try
            {
                //TODO: replace with COL.
                var lst = await _db.BookEdition.Where(b => bookeditionIds.Contains(b.bookEditionid)).
                    OrderBy(o => o.bookOrder).Select(c => new BookEdition()
                    {
                        bookEditionid = c.bookEditionid,
                        EnglishTitle = c.EnglishTitle,
                        title = c.title,
                        isbn = c.isbn
                    }).ToArrayAsync();

                return Ok(lst);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failure {0}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpGet("GetVersToolTip/{textid}/{langid}")]
        public async Task<IActionResult> GetVersToolTip(int textid, int langid)
        {
            try
            {
                var data = await _db.Text.Where(w => w.textid == textid)
                    .Select(s => new { s.Alineaid, s.bookedition, s.bookchapteralinea.bookchapter.chapter }).FirstOrDefaultAsync();
                if (data != null)
                {

                    // var bc = _db.BookChapter[bca.BookchapterId];
                    // var booked = _db.Contents.BookEditions[text.bookeditionid];

                    var retVal = new
                    {
                        Book = data.bookedition.title,
                        Chapter = data.chapter,
                        TextId = textid,
                        Verse = data.Alineaid,
                        BookEnglish = data.bookedition.EnglishTitle
                    };

                    return Ok(retVal);

                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failure {0}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
        [HttpGet("GetEnglishTitleFromBookEdition/{bookeditionId}")]
        public async Task<IActionResult> GetEnglishTitleFromBookEdition(int bookeditionId)
        {
            try
            {
                var res = await _db.BookEdition.
                    Where(w => w.bookEditionid == bookeditionId).
                    Select(s => s.EnglishTitle).FirstOrDefaultAsync();

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failure {0}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            
        }
    }
}