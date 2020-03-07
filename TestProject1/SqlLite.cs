using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Peshitta.Infrastructure.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using System.Data.Common;
using Peshitta.Infrastructure.DB;
using System.Diagnostics;
using AutoMapper;
using System.Linq;
using Peshitta.Infrastructure;

namespace TestProject1
{
    [TestClass]
    public class SqlLite
    {

        KitabDB kitabDb;
        BijbelRepository _repo;
        string baseF = @"c:\temp\bijbel";
        IMapper mapper;
        DbSqlContext dbSqlContext;
        [TestInitialize]
        public void Init()
        {

            var connectionString = ConfigurationManager.ConnectionStrings["bijbel"].ConnectionString;

            var _options = new DbContextOptionsBuilder<DbSqlContext>()
                .UseSqlite(connectionString)
                .Options;
            dbSqlContext = new DbSqlContext(_options);
            _repo = new BijbelRepository(dbSqlContext);
            try
            {

                dbSqlContext.Database.EnsureDeleted();
                dbSqlContext.Database.EnsureCreated();
            }
            catch (InvalidOperationException inv)
            {

                throw inv;
            }
            var mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Peshitta.Infrastructure.Models.Book, Peshitta.Infrastructure.Sqlite.Model.Book>()
                .ForMember(m => m.abbrevation, opt => opt.MapFrom(m => m.abbreviation == null ? m.Title.Substring(0, 2) : m.abbreviation));
                cfg.CreateMap<Peshitta.Infrastructure.Models.BookEdition, Peshitta.Infrastructure.Sqlite.Model.bookedition>().
                ForMember(m => m.keywords, opt => opt.MapFrom(map => map.keywords != null ? string.Join(",", map.keywords): null));
                cfg.CreateMap<Peshitta.Infrastructure.Models.Text, Peshitta.Infrastructure.Sqlite.Model.Text>();
                cfg.CreateMap<Peshitta.Infrastructure.Models.words, Peshitta.Infrastructure.Sqlite.Model.words>();
                cfg.CreateMap<Peshitta.Infrastructure.Models.BookChapter, Peshitta.Infrastructure.Sqlite.Model.BookChapter>();
                cfg.CreateMap<Peshitta.Infrastructure.Models.BookChapterAlinea, Peshitta.Infrastructure.Sqlite.Model.BookChapterAlinea>();
                cfg.CreateMap<Peshitta.Infrastructure.Models.TextWords, Peshitta.Infrastructure.Sqlite.Model.TextWords>();
                cfg.CreateMap<Peshitta.Infrastructure.Models.TextWordsHistory, Peshitta.Infrastructure.Sqlite.Model.TextWordsHistory>();
            });

            mapper = mapConfig.CreateMapper();
            var start = Environment.TickCount;
            kitabDb = KitabDB.LoadFromDiskAsync(baseF, false).Result;
            Debug.WriteLine("Loading KitabDB took {0} ms", Environment.TickCount - start);

            start = Environment.TickCount;
            kitabDb = KitabDB.LoadFromDiskAsync(baseF, false).Result;
            Debug.WriteLine("Loading KitabDB including the cache took {0} ms", Environment.TickCount - start);

        }
        [TestMethod]
        public void Muke()
        {
            var key = new Peshitta.Infrastructure.Models.WordLanguageKey("Piet", 19);
            var hash = key.GetHashCode();
            var key2 = new Peshitta.Infrastructure.Models.WordLanguageKey("Piet", 90);
            var hash2 = key2.GetHashCode();
            Assert.AreNotEqual(hash, hash2);
        }
        //[TestMethod]
        //public async Task ConVertColumn()
        //{
        //    var data = await dbSqlContext.Text.ToArrayAsync();

        //    foreach (var dat in data)
        //    {
        //        dat.timestamp2 = dat.timestamp;
                
        //    }
        //    dbSqlContext.UpdateRange(data);
        //    await dbSqlContext.SaveChangesAsync();
        //}
        //[TestMethod]
        //public async Task TestSelect()
        //{
        //    var data = await dbSqlContext.Text.Where(w => w.textid == 2)
        //         .Select(s => new { s.Alineaid, s.bookedition, s.bookchapteralinea.bookchapter.chapter }).FirstOrDefaultAsync();
        //        Assert.IsNotNull(data);
        //}
        [TestMethod]
		public async Task TestCreate()
		{
			var book =await dbSqlContext.Books.FirstOrDefaultAsync();

            foreach (var b in kitabDb.Contents.Books)
            {
                dbSqlContext.Add(mapper.Map<Peshitta.Infrastructure.Sqlite.Model.Book>(b.Value));
              //  await dbSqlContext.SaveChangesAsync();
            }


            foreach (var be in (await kitabDb.BookEditions).Data)
            {
                var beC = mapper.Map<Peshitta.Infrastructure.Sqlite.Model.bookedition>(be);
                //beC.bookEditionid = be.Key;
                dbSqlContext.Add(beC);
            }
           
            dbSqlContext.Publications.AddRange(new Peshitta.Infrastructure.Sqlite.Model.Publication { Code = "AB", Name = "Peshitta" },
                new Peshitta.Infrastructure.Sqlite.Model.Publication { Code = "PS", Name = "Syriac Peshitta" });
            var idsExclude = new[] { 6353, 4985, 4771, 8531 , 3533, 8471, -18867, 7353 };
            foreach (var w in kitabDb.Contents.Words.Where(w => !idsExclude.Contains(w.Key)))
            {
                var mapped = mapper.Map<Peshitta.Infrastructure.Sqlite.Model.words>(w.Value);
                mapped.LangId = w.Value.LangId;
               // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))
               if (mapped.id>= -15146)
                {
                    mapped.LangId = 90;
                }
                dbSqlContext.Add(mapped);            
                //var p = await dbSqlContext.Words.FirstOrDefaultAsync();
            }
            await dbSqlContext.SaveChangesAsync();
            foreach (var w in kitabDb.Contents.BookChapters)
            {
                var mapped = mapper.Map<Peshitta.Infrastructure.Sqlite.Model.BookChapter>(w.Value);
              
                // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))
                dbSqlContext.Add(mapped);
                //var p = await dbSqlContext.Words.FirstOrDefaultAsync();
            }
            await dbSqlContext.SaveChangesAsync();

           
            foreach (var w in kitabDb.Contents.BookChapterAlineas)
            {
                var mapped = mapper.Map<Peshitta.Infrastructure.Sqlite.Model.BookChapterAlinea>(w.Value);
                mapped.bookchapteralineaid = w.Key.bookchapteralineaid;
                mapped.Alineaid = w.Key.Alineaid;
                // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))

                dbSqlContext.Add(mapped);
                //   await dbSqlContext.SaveChangesAsync();
                //var p = await dbSqlContext.Words.FirstOrDefaultAsync();
            }
            await dbSqlContext.SaveChangesAsync();
            foreach (var be in (await kitabDb.BookEditions).Data)
            {
                var beC = await dbSqlContext.BookEdition.FindAsync(be.bookEditionid);
                foreach (var chap in (await kitabDb.ChaptersByBookIdAsync(be.bookEditionid)))
                {
                    if (be.langid == 19)
                    {
                     
                        kitabDb.ActivePublications = new []{ "AB" };
                    }
                    else
                    {
                        kitabDb.ActivePublications = new []{ "PS" };
                    }
                    foreach (var ta in (await kitabDb.LoadChapterAsync(chap.Key.chapter, be.bookEditionid)).Data)
                    {

                        foreach(var t in ta.Texts)
                        {
                            Trace.TraceInformation("Chap {0}, bookedition id {1}, verse {2}", chap.Key.chapter, be.bookEditionid, t.Alineaid);
                            var text = new Peshitta.Infrastructure.Sqlite.Model.Text
                            {
                                Alineaid = t.Alineaid,
                                bookeditionid = t.bookeditionid,
                                textid = t.TextId,
                                timestamp = t.timestamp,
                                BookChapterAlineaid = ta.BookChapterAlineaId,
                                 
                            };
                            text.bookedition = beC;
                            text.bookeditionid = beC.bookEditionid;
                           
                            var plain = t.Content;
                            var remarks = t.Remarks;
                            dbSqlContext.Text.Add(text);
                            await dbSqlContext.SaveChangesAsync();
                            foreach (var history in t.Histories)
                            {
                                 if (history.ArchiveDate == DateTime.MinValue)
                                {
                                    continue;
                                }
                                //await _repo.CompressVerse(t.TextId, history.ArchiveDate, history.expanded.Content, history.expanded.Remarks, null);
                            }
                            await _repo.CompressVerse(t.TextId, t.timestamp, t.Content, t.Remarks, null);

                        }
                    }
                }
            }

            
		}
	}
}
