using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using peshitta.nl.Api;
using Peshitta.Infrastructure;
using Peshitta.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using peshitta.nl.Api.Helpers;
using System.IO;
using Microsoft.Net.Http.Headers;
using peshitta.nl.Api.Models;

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
    [HttpGet("GetVerse/{pTextId}"), Obsolete("Uses SqliteModel directly use GetVerses")]
    [ProducesResponseType(StatusCodes.Status200OK)] // do not add type, Swashbuckle will fail
    public async Task<ActionResult> GetVerse(int pTextId)
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

    [AcceptVerbs("get", "head", Route ="Download/{file}")]
		public IActionResult Get(string file, [FromServices] PathHelper _pathHelper, [FromServices] Options _options)
		{
			if (string.IsNullOrEmpty(file))
			{
				return BadRequest();
			}
			if (file.IndexOf('%') > 0)
			{
				file = System.Web.HttpUtility.UrlDecode(file);
			}

			var mimeType = DataMapper.ExtensionMap(file);
			
			
			var uploadedPath = _pathHelper.ExpandPath( _options.MediaPath );
			
			var f = Path.Combine(uploadedPath, file);
			var fi = new FileInfo(f);
			if (!fi.Exists)
			{
				return NotFound();
			}
		
			//avoid file exposure above our website
			if (!fi.DirectoryName.StartsWith(_pathHelper.GetContentRootPath(), StringComparison.OrdinalIgnoreCase))
			{
				return Forbid();
			}
			_logger.LogTrace("Returning file {0}", fi.FullName);

	
			var result = PhysicalFile(fi.FullName, mimeType, new DateTimeOffset(fi.LastWriteTimeUtc, TimeSpan.Zero),
				new EntityTagHeaderValue($"\"{Path.GetFileNameWithoutExtension(f)}\""));
			Response.Headers["Cache-Control"] = $"Max-Age={TimeSpan.FromDays(30).TotalSeconds}";
			return result;
		}
    
    /// <summary>
    /// Get multiple verses in one GET operation
    /// </summary>
    /// <param name="textid">an array of textid=number tuples</param>
    [HttpGet("GetVerses")]
    [ProducesResponseType(typeof(IEnumerable<TextExpanded>),  StatusCodes.Status200OK)]
    public async Task<ActionResult> GetVerses([FromQuery] IEnumerable<int> textid)
    {
      try
      {

        var result = new List<TextExpanded>(textid.Count());
        foreach(var t in textid)
          result.Add(  (await _repo.DecompressVerse(t)).ToDtoModelExpanded());
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError("Failure {0}", ex);
        return StatusCode(StatusCodes.Status500InternalServerError);
      }

    }
    [HttpGet("publications")]
    [ProducesResponseType(typeof(IEnumerable<Peshitta.Infrastructure.Models.Publication>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Publications()
    {
      var result = await _repo.PublicationCodes();
      return Ok(result);
    }
    [HttpGet("config")] //staticfiles does not work nicely with cors
    public async Task<ActionResult> GetConfig()
    {
      return Content(await System.IO.File.ReadAllTextAsync("wwwroot/config/appConfig.json"), "application/json");
    }
    [HttpGet("BookMetaData")]
    public async Task<ActionResult> GetBookMetaData([FromQuery] IEnumerable<string> pub)
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
    public async Task<ActionResult> GetVerseHistory(int pTextId)
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
    public async Task<ActionResult> CompareTimeStamp([FromBody] TimestampParams pars)
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
          retVal[i] = pars.pTimeStamp[i].CompareTo(timeStamps[i]);
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
    public async Task<ActionResult> UpdateVerse([FromBody] VerseTemp vTemp)
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
