
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Peshitta.Infrastructure;

namespace peshitta.nl.Api
{
  [ApiController]
  public class VersingController(BijbelRepository _repo, ILogger<VersingController> _logger) : ControllerBase
  {


    [HttpGet("GetBookEditionsFromPub/{pubid}")]
    public async Task<IActionResult> GetBookEditionsFromPub(string pubid)
    {
      try
      {
        var list = await _repo.BookEditionsByPublicationCode(pubid);

        return Ok(list);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,"Failure");
        return StatusCode(StatusCodes.Status500InternalServerError);
      }

    }
    //[HttpGet]
    //public async Task<IActionResult> GetChapterAlineas(IEnumerable<int> bookeditionIds)
    //{
    //  try
    //  {
    //    //TODO: replace with COL.
    //    var lst = await _repo.GetChapterAlineaInfoFromBookEditionId(bookeditionIds);

    //    return Ok(lst);
    //  }
    //  catch (Exception ex)
    //  {
    //    _logger.LogError("Failure {0}", ex);
    //    return StatusCode(StatusCodes.Status500InternalServerError);
    //  }
    //}
    [HttpPost("GetBookInfos")]
    public async Task<IActionResult> GetBookInfos([FromBody] IEnumerable<int> bookeditionIds)
    {
      try
      {
        //TODO: replace with COL.
        var lst =await  _repo.BookInfoByBookeditionIds(bookeditionIds);

        return Ok(lst);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,"Failure ");
        return StatusCode(StatusCodes.Status500InternalServerError);
      }

    }

    [HttpGet("GetVersToolTip/{textid}/{langid}")]
    public async Task<IActionResult> GetVersToolTip(int textid, int langid)
    {
      try
      {
        var data = await _repo.GetVerseToolTip(textid, langid);
        if (data!=null) {
          return Ok(data);

        }
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failure");
        return StatusCode(StatusCodes.Status500InternalServerError);
      }

    }
    [Obsolete("Use BookInfo")]
    [HttpGet("GetEnglishTitleFromBookEdition/{bookeditionId}")]
    public async Task<IActionResult> GetEnglishTitleFromBookEdition(int bookeditionId)
    {
      try
      {
        var res = await _repo.BookInfoByBookeditionIds([bookeditionId]);
        if (res?.Any() ?? false)
        {
          var data = res.First();
          return Ok(data.EnglishTitle);
        }

        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failure");
        return StatusCode(StatusCodes.Status500InternalServerError);
      }

    }
  }
}
