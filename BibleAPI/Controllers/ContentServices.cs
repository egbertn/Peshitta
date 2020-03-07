using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using peshitta.nl.Api;
using Peshitta.Infrastructure;
using Peshitta.Infrastructure.Sqlite;

namespace peshitta.nl
{


  [Route("[controller]")]
  public class ContentController : ControllerBase
  {

    private readonly ILogger<ContentController> _logger;
    //TODO create service + repository etc
    private readonly BijbelRepository _repo;
    public ContentController(BijbelRepository db, ILogger<ContentController> logger)
    {
      _repo = db;
      _logger = logger;
    }
    [HttpGet("GetVerse/{pTextId}")]
    public async Task<IActionResult> GetVerse(int pTextId)
    {
      try
      {
        var text = await _repo.DecompressVerse(pTextId);
        return Ok(text);
      }
      catch (Exception ex)
      {
        _logger.LogError("Failure {0}", ex);
        return StatusCode(StatusCodes.Status500InternalServerError);
      }

    }
    [HttpGet("BookMetaData")]
    public async Task<IActionResult> GetBookMetaData([FromQuery] IEnumerable<string> pub)
    {
      if (pub == null || !pub.Any())
      {
        return BadRequest();
      }
      try
      {
        var result = await _repo.MetaDataForPublications(pub);
        return Ok(result);
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
        var text = await _repo.GetVerseHistory(pTextId);
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

        var timeStamps = await _repo.GetVerseTimeStamps(pars.pTextId);

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
    //TODO: authorization
    [HttpPost("UpdateVerse")]
    public async Task<IActionResult> UpdateVerse([FromBody] VerseTemp vTemp)
    {
      try
      {
        var didAdd = await _repo.CompressVerse(vTemp.pTextId, vTemp.pTimeStamp, vTemp.pContent, vTemp.pFootNoteText, vTemp.pHeaderText);
        return NoContent();


      }
      catch (Exception ex)
      {
        _logger.LogError("Failure {0}", ex);
        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
      }
    }


  }
}
