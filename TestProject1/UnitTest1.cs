using Microsoft.VisualStudio.TestTools.UnitTesting;
using peshitta.nl.DB;
using Peshitta.Data.Data;
using Peshitta.Data.DB;
using Peshitta.Data.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject1
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        string baseF = @"c:\temp\bijbel";

        KitabDB kitabDb;
        [TestInitialize]
        public void Start()
        {
            var start = Environment.TickCount;
            kitabDb = KitabDB.LoadFromDiskAsync(baseF, false).Result;
            Debug.WriteLine("Loading KitabDB took {0} ms", Environment.TickCount - start);

            start = Environment.TickCount;
            kitabDb = KitabDB.LoadFromDiskAsync(baseF, false).Result;
            Debug.WriteLine("Loading KitabDB including the cache took {0} ms", Environment.TickCount - start);
        }
        [TestMethod]
        public void GetUTf()
        {
            var dt = DateTime.Parse("2015-08-30T10:17:07.5275458+02:00", null, System.Globalization.DateTimeStyles.AssumeUniversal);
            var utf = dt.ToUniversalTime();
            var piet = Newtonsoft.Json.JsonConvert.SerializeObject(new { dt = DateTime.Now });
        }

        [TestMethod]
        public async Task MustLoadFromDisk()
        {
            var baseF = @"c:\temp\bijbel";
            Trace.TraceInformation("Starting...");
            var kitabDb = await KitabDB.LoadFromDiskAsync(baseF, false);
  
            var words = kitabDb.Contents.Words;

        }
        [TestMethod]
        public async Task MustLoadChapterFromDisk()
        {


            kitabDb.ActivePublications = new[] { "AB", "PS" };
            var start = Environment.TickCount;
            var data = (await kitabDb.LoadChapterAsync(1, 1));
            var chapterCtx = data.Data.ToArray();
            Debug.WriteLine("Loading chapters with text took {0}ms", Environment.TickCount - start);
          

        }
        [TestMethod]
        public async Task SearchSucceeds()
        {
            var args = new SearchArguments {ExactMatch=false, FindString="de heer", PageIndex=0, PageSize=10 };
            kitabDb.ActivePublications = new [] { "AB" };
            var page = 0;
            Response<string> results=null;
            var totalC = 0;
            var start = Environment.TickCount;
            while (page >= 0)
            {
                args.PageIndex = page++;
                results = await kitabDb.Search(args);
                totalC += results.Data.Count();
                if (!results.Data.Any())
                {
                    break;
                }
            }
            Assert.IsNotNull(results, "Search should have a result");
            Assert.AreEqual(totalC, 632, "Search should ahve a result");
            Debug.WriteLine("searching using paging took {0}ms", Environment.TickCount - start);

            page = 0;
            args.PageSize = totalC;
            args.PageIndex = 0;
            totalC = 0;
             start = Environment.TickCount;
            while (page >= 0)
            {
                args.PageIndex = page++;
                results = await kitabDb.Search(args);
                totalC += results.Data.Count();
                if (!results.Data.Any())
                {
                    break;
                }
            }
            Debug.WriteLine("searching using paging took {0}ms", Environment.TickCount - start);
        }
        [TestMethod]
        public void LoadVerseHistory()
        {
            
            var verses = kitabDb.LoadHistory(5350);
            Assert.IsNotNull(verses, "We should have a collection");
            Assert.IsTrue(verses.Any(), "We should have history");
        }
        [TestMethod]
        public async Task MustLoadChaptersFromBookId()
        {
            var chapters = (await kitabDb.ChaptersByBookIdAsync(1)).ToArray();
            Assert.AreEqual(chapters.Length, 28, "Problem");
        }

        //[TestMethod, Ignore]
        //public async Task SerialiseToDisk()
        //{
        //    using (var db = new BookDal())
        //    {
        //        //var bookchaps = db.getChaptersByBookId(1).ToArray();
        //        Debug.WriteLine("Starting...");
        //        var contents = db.SerializeDB("PS", "AB");
        //        var kitabDB = new KitabDB(contents);
        //        await kitabDB.BackupToDiskAsync(baseF);
        //    }
        //    var tilhere = new Random().Next();
        //    Debug.WriteLine("Done, press any key...");


        //}
    }
}
