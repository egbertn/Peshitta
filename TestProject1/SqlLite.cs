using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Peshitta.Data.SqlLite;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using System.Data.Common;
using Peshitta.Data.DB;
using System.Diagnostics;
using AutoMapper;
using System.Linq;

namespace TestProject1
{
	[TestClass]
	public class SqlLite
	{

        KitabDB kitabDb;
        DbSqlContext dbSqlContext;
        string baseF = @"c:\temp\bijbel";
        IMapper mapper;
        [TestInitialize]
		public void Init()
		{
			
			var connectionString= ConfigurationManager.ConnectionStrings["bijbel"].ConnectionString;
			
			var _options = new DbContextOptionsBuilder<DbSqlContext>()
				.UseSqlite(connectionString)
				.Options;

			dbSqlContext = new DbSqlContext(_options);
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
                cfg.CreateMap<Peshitta.Data.Models.Book, Peshitta.Data.SqlLite.Model.Book>()
                .ForMember(m => m.abbrevation, opt => opt.MapFrom(m => m.abbreviation == null ? m.Title.Substring(0, 2): m.abbreviation));
                cfg.CreateMap<Peshitta.Data.Models.BookEdition, Peshitta.Data.SqlLite.Model.bookedition>();
                cfg.CreateMap<Peshitta.Data.Models.Text, Peshitta.Data.SqlLite.Model.Text>();
                cfg.CreateMap<Peshitta.Data.Models.words, Peshitta.Data.SqlLite.Model.words>();
                cfg.CreateMap<Peshitta.Data.Models.BookChapter, Peshitta.Data.SqlLite.Model.BookChapter>();
                cfg.CreateMap<Peshitta.Data.Models.BookChapterAlinea, Peshitta.Data.SqlLite.Model.BookChapterAlinea>();
                cfg.CreateMap<Peshitta.Data.Models.TextWords, Peshitta.Data.SqlLite.Model.TextWords>();
                cfg.CreateMap<Peshitta.Data.Models.TextWordsHistory, Peshitta.Data.SqlLite.Model.TextWordsHistory>();
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
		public async Task TestCreate()
		{
			var book =await dbSqlContext.Books.FirstOrDefaultAsync();

            foreach (var b in kitabDb.Contents.Books)
            {
                await dbSqlContext.Books.AddAsync(mapper.Map<Peshitta.Data.SqlLite.Model.Book>(b.Value));
              //  await dbSqlContext.SaveChangesAsync();
            }


            foreach (var be in (await kitabDb.BookEditions).Data)
            {
                var beC = mapper.Map<Peshitta.Data.SqlLite.Model.bookedition>(be);
                //beC.bookEditionid = be.Key;
                await dbSqlContext.BookEdition.AddAsync(beC);
            }
            foreach(var w in kitabDb.Contents.Words)
            {
                var mapped = mapper.Map<Peshitta.Data.SqlLite.Model.words>(w.Value);
                mapped.LangId = w.Value.LangId;
               // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))
                await dbSqlContext.Words.AddAsync(mapped);            
                //var p = await dbSqlContext.Words.FirstOrDefaultAsync();
            }
            await dbSqlContext.SaveChangesAsync();
            foreach (var w in kitabDb.Contents.BookChapters)
            {
                var mapped = mapper.Map<Peshitta.Data.SqlLite.Model.BookChapter>(w.Value);
              
                // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))
                await dbSqlContext.BookChapter.AddAsync(mapped);
                //var p = await dbSqlContext.Words.FirstOrDefaultAsync();
            }
            await dbSqlContext.SaveChangesAsync();

           
            foreach (var w in kitabDb.Contents.BookChapterAlineas)
            {
                var mapped = mapper.Map<Peshitta.Data.SqlLite.Model.BookChapterAlinea>(w.Value);
                mapped.bookchapteralineaid = w.Key.bookchapteralineaid;
                mapped.Alineaid = w.Key.Alineaid;
                // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))

                await dbSqlContext.BookchapterAlinea.AddAsync(mapped);
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
                            var text = new Peshitta.Data.SqlLite.Model.Text
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
                            await dbSqlContext.CompressVerse(text, plain, remarks, null);
                            await dbSqlContext.Text.AddAsync(text);
                        }
                    }
                }
            }

            await dbSqlContext.SaveChangesAsync();
		}
	}
}
