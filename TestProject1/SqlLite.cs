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
using Peshitta.Infrastructure.Models;

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
        DbSqlContext dbSqliteContext;
        [TestInitialize]
        public void Init()
        {

            var connectionString = ConfigurationManager.ConnectionStrings["bijbel2"].ConnectionString;
            var connectionStringLite = ConfigurationManager.ConnectionStrings["bijbel"].ConnectionString;
            //var _options = new DbContextOptionsBuilder<DbSqlContext>()
            //    .UseSqlite(connectionString)
            //    .Options;
            var _options = new DbContextOptionsBuilder<DbSqlContext>()
                .UseSqlServer(connectionString)
                .Options;
            dbSqlContext = new DbSqlContext(_options);
            var optionsLite = new DbContextOptionsBuilder<DbSqlContext>().UseSqlite(connectionStringLite).Options;
            dbSqliteContext = new DbSqlContext(optionsLite);

            _repo = new BijbelRepository(dbSqliteContext);
            try
            {
                //dbSqliteContext.Database.EnsureDeleted();
                //dbSqliteContext.Database.EnsureCreated();
            }
            catch (InvalidOperationException inv)
            {

                throw inv;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            try
            {

                dbSqlContext.Database.EnsureDeleted();
                dbSqlContext.Database.EnsureCreated();
            }
            catch (InvalidOperationException inv)
            {

                throw inv;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            var mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Peshitta.Infrastructure.Models.Book, Peshitta.Infrastructure.Sqlite.Model.Book>()
                .ForMember(m => m.abbrevation, opt => opt.MapFrom(m => m.abbreviation ?? m.Title.Substring(0, 2)));
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
        public async Task Copy()
        {

   //         var pubs = await dbSqlContext.Publications.ToArrayAsync();
			//dbSqlContext.Publications.RemoveRange(dbSqlContext.Publications);
			//dbSqliteContext.AddRange(pubs);
   //         await dbSqliteContext.SaveChangesAsync();

   //         var words = await dbSqlContext.Words.ToArrayAsync();
			//dbSqliteContext.Words.RemoveRange(dbSqliteContext.Words);
			//dbSqliteContext.AddRange(words);
   //         await dbSqliteContext.SaveChangesAsync();
			
   //         var books = await dbSqlContext.Books.ToArrayAsync();
			//dbSqliteContext.Books.RemoveRange(dbSqliteContext.Books);
			//dbSqliteContext.Books.AddRange(books);
   //         await dbSqliteContext.SaveChangesAsync();

   //         var be = await dbSqlContext.BookEdition.ToArrayAsync();
			//dbSqliteContext.BookEdition.RemoveRange(dbSqliteContext.BookEdition);
			//dbSqliteContext.AddRange(be);
   //         await dbSqliteContext.SaveChangesAsync();

   //         var chapters = await dbSqlContext.BookChapter.ToArrayAsync();
			//dbSqliteContext.BookChapter.RemoveRange(dbSqliteContext.BookChapter);
			////foreach(var ch in chapters)
			////{
			////	dbSqliteContext.BookChapter.Add(ch);
			////	try
			////	{
			////		await dbSqliteContext.SaveChangesAsync();
			////	}
			////	catch(Exception ex)
			////	{
			////		Trace.TraceError("Exception {0}", ex);
			////	}
			////}
			//dbSqliteContext.BookChapter.AddRange(chapters);
   //         await dbSqliteContext.SaveChangesAsync();

   //         var chaalin = await dbSqlContext.BookchapterAlinea.ToArrayAsync();
			//dbSqliteContext.BookchapterAlinea.RemoveRange(dbSqliteContext.BookchapterAlinea);
			//dbSqliteContext.AddRange(chaalin);
   //         await dbSqliteContext.SaveChangesAsync();

            var text = await dbSqlContext.Text.ToArrayAsync();
			await dbSqliteContext.Database.ExecuteSqlRawAsync("DELETE FROM Text WHERE TextId>=13117;");
            dbSqliteContext.AddRange(text);
            await dbSqliteContext.SaveChangesAsync();

            var tw = await dbSqlContext.TextWords.OrderBy(t => t.textid).ThenBy(o=> o.id).ToArrayAsync();
            dbSqliteContext.TextWords.RemoveRange(dbSqliteContext.TextWords.Where(w => w.textid >= 13117).ToArray());
            //	await dbSqliteContext.Database.ExecuteSqlRawAsync("DELETE FROM TextWords;");
            var maxTWID = await dbSqliteContext.TextWords.MaxAsync(m => m.id);
            foreach(var t in tw)
            {
                t.id = ++maxTWID;
            }
            dbSqliteContext.TextWords.AddRange(tw);
            await dbSqliteContext.SaveChangesAsync();

            //var twh = await dbSqlContext.TextwordsHistory.ToArrayAsync();
            ////await dbSqliteContext.Database.ExecuteSqlRawAsync("DELETE FROM TextWordsHistory");
            //dbSqliteContext.TextwordsHistory.RemoveRange(dbSqliteContext.TextwordsHistory);
            //dbSqliteContext.TextwordsHistory.AddRange(twh);
            //foreach (var twhSub in twh)
            //{
            //    dbSqliteContext.TextwordsHistory.Add(twhSub);
            //    try
            //    {


            //        await dbSqliteContext.SaveChangesAsync();
            //    }
            //    catch(Exception ex)
            //    {
            //        throw ex;
            //    }
            //}
			//dbSqliteContext.TextwordsHistory.AddRange(twh);
            await dbSqliteContext.SaveChangesAsync();
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
        [TestMethod]
        public async Task RemoveDuplicates()
        {
       


            var optionsLite = new DbContextOptionsBuilder<DbSqlContext>().UseSqlite(dbSqliteContext.Database.GetDbConnection().ConnectionString).Options;
           var newCn = new DbSqlContext(optionsLite);
            var cmd = dbSqliteContext.Database.GetDbConnection().CreateCommand();
            await cmd.Connection.OpenAsync();
            await newCn.Words.FirstOrDefaultAsync();
            var cmdWithHash = dbSqliteContext.Database.GetDbConnection().CreateCommand(); 
            cmd.CommandText = "SELECT DISTINCT wordid FROM textwords WHERE wordid in (select id from words where hash IS NULL AND isnumber = 0)";
            cmdWithHash.CommandText = "SELECT id FROM words WHERE word=@w AND NOT hash IS NULL";
            var par = cmdWithHash.CreateParameter();
            par.ParameterName = "@w";
            par.DbType = System.Data.DbType.String;
            cmdWithHash.Parameters.Add(par);
            var data = await cmd.ExecuteReaderAsync();

           while(await data.ReadAsync())
            {   
                var d = data.GetInt32(0);
                var wordRecord = await dbSqliteContext.Words.FindAsync(d);
                par.Value = wordRecord.word;
                try
                {
                    var wid = await cmdWithHash.ExecuteScalarAsync();
                    await dbSqliteContext.Database.ExecuteSqlRawAsync($"UPDATE textwords SET wordid={wid} WHERE wordid={d}");
                    dbSqliteContext.Database.ExecuteSqlRaw($"UPDATE textwordshistory SET wordid={wid} WHERE wordid={d}");
                }
                catch(Exception ex)
                {
                    var msg = ex.Message;
                }
            }    
           //TODO fix words to be lang 19
        }
        //[TestMethod]
        //public async Task TestSelect()
        //{
        //    var data = await dbSqlContext.Text.Where(w => w.textid == 2)
        //         .Select(s => new { s.Alineaid, s.bookedition, s.bookchapteralinea.bookchapter.chapter }).FirstOrDefaultAsync();
        //        Assert.IsNotNull(data);
        //}
        [TestMethod]
        public async Task UpdateLanguage()
        {
            var words = dbSqliteContext.Words;
            foreach (var w in words.Where(w => w.LangId == 90 && w.IsNumber == true).ToArray())
            {
                w.LangId = 19;
                dbSqliteContext.Words.Update(w);
            }
            foreach (var w in words.Where(w => w.LangId == 90 && w.IsNumber == false).ToArray().Where(w=>ContainsAramaic(w.word) == false))
            {
                w.LangId = 19;
                dbSqliteContext.Words.Update(w);
            }
            await dbSqliteContext.SaveChangesAsync();
        }
        private static bool ContainsAramaic(string word)
        {
            return word.Any(a => (short)a > 1800 && (short)a < 2000);
        }
        [TestMethod]
        public async Task UpdateHash()
        {
            var words = dbSqlContext.Words;
            foreach(var word in words.Where(w => w.IsNumber == false))
            {
                var hash = new WordLanguageKey(word.word, word.LangId).GetHashCode();
                if (word.hash != hash)
                {
                    word.hash = hash;
                    dbSqlContext.Words.Update(word);
                }
            }
            await dbSqlContext.SaveChangesAsync();
           
            return; words = dbSqliteContext.Words;
            var t = 0;
            foreach (var word in words.Where(w => !w.IsNumber))
            {
                if (word.hash == null)
                {
                    word.hash = new WordLanguageKey(word.word, word.LangId).GetHashCode();
                        dbSqliteContext.Words.Update(word);    
                    try
                    {
                        await dbSqliteContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        var x = 0;
                    }
                }
                // if (t % 10 == 0)
            
                t++;
            }
             dbSqliteContext.SaveChanges();
        }
        [TestMethod]
		public async Task TestCreate()
		{
			//var book =await dbSqlContext.Books.FirstOrDefaultAsync();

   //         foreach (var b in kitabDb.Contents.Books)
   //         {
   //             dbSqlContext.Add(mapper.Map<Peshitta.Infrastructure.Sqlite.Model.Book>(b.Value));
   //           //  await dbSqlContext.SaveChangesAsync();
   //         }

   //         await dbSqlContext.SaveChangesAsync();
   //         foreach (var be in (await kitabDb.BookEditions).Data)
   //         {
   //             var beC = mapper.Map<Peshitta.Infrastructure.Sqlite.Model.bookedition>(be);
   //             //beC.bookEditionid = be.Key;
   //             dbSqlContext.Add(beC);
   //         }
   //         await dbSqlContext.SaveChangesAsync();
   //         dbSqlContext.Publications.AddRange(new Peshitta.Infrastructure.Sqlite.Model.Publication { Code = "AB", Name = "Peshitta" },
   //             new Peshitta.Infrastructure.Sqlite.Model.Publication { Code = "PS", Name = "Syriac Peshitta" });
   //         var idsExclude = new[] { 6353, 4985, 4771, 8531 , 3533, 8471, -18867, 7353 };
   //         foreach (var w in kitabDb.Contents.Words.Where(w => !idsExclude.Contains(w.Key)))
   //         {
   //             var mapped = mapper.Map<Peshitta.Infrastructure.Sqlite.Model.words>(w.Value);
   //             mapped.LangId = w.Value.LangId;
   //            // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))
   //            if (mapped.id>= -15164)
   //             {
   //                 mapped.LangId = 90;
   //                 mapped.hash = new WordLanguageKey(mapped.word, mapped.LangId).GetHashCode();
   //             }
   //             dbSqlContext.Add(mapped);              

   //             //var p = await dbSqlContext.Words.FirstOrDefaultAsync();
   //         }
   //         await dbSqlContext.SaveChangesAsync();
   //         foreach (var w in kitabDb.Contents.BookChapters)
   //         {
   //             var mapped = mapper.Map<Peshitta.Infrastructure.Sqlite.Model.BookChapter>(w.Value);
              
   //             // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))
   //             dbSqlContext.Add(mapped);
   //             //var p = await dbSqlContext.Words.FirstOrDefaultAsync();
   //         }
   //         await dbSqlContext.SaveChangesAsync();

           
   //         foreach (var w in kitabDb.Contents.BookChapterAlineas)
   //         {
   //             var mapped = mapper.Map<Peshitta.Infrastructure.Sqlite.Model.BookChapterAlinea>(w.Value);
   //             mapped.bookchapteralineaid = w.Key.bookchapteralineaid;
   //             mapped.Alineaid = w.Key.Alineaid;
   //             // if (!(await dbSqlContext.Words.ContainsAsync(mapped)))

   //             dbSqlContext.Add(mapped);
   //             //   await dbSqlContext.SaveChangesAsync();
   //             //var p = await dbSqlContext.Words.FirstOrDefaultAsync();
   //         }
   //         await dbSqlContext.SaveChangesAsync();
            //delete FROM text where textid >=13117
            foreach (var be in (await kitabDb.BookEditions).Data)
            {
                var beC = await dbSqliteContext.BookEdition.FindAsync(be.bookEditionid);
                foreach (var chap in (await kitabDb.ChaptersByBookIdAsync(be.bookid)))
                {
                   
                    if (be.langid == 19)
                    {
                     
                        kitabDb.ActivePublications = new []{ "AB" };
                        continue;
                    }
                    else
                    {
                        kitabDb.ActivePublications = new []{ "PS" };
                    }
                    foreach (var ta in (await kitabDb.LoadChapterAsync(chap.Key.chapter, be.bookEditionid)).Data)
                    {

                        foreach(var t in ta.Texts)
                        { 
                            //if (t.BookChapterAlineaid <= 6850) // be.bookEditionid == 1 && ( chap.Key.chapter < 27 || (chap.Key.chapter == 27 && t.Alineaid<=30))
                            //{
                            //    continue;
                            //}
                            if (await dbSqliteContext.Text.AsNoTracking().AnyAsync(a => a.textid == t.TextId ))
                            {
                                continue;
                            }
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
                            dbSqliteContext.Text.Add(text);
                            await dbSqliteContext.SaveChangesAsync();
                            foreach (var (ArchiveDate, expanded) in t.Histories)
                            {
                                 if (ArchiveDate == DateTime.MinValue)
                                {
                                    continue;
                                }
                                await _repo.CompressVerse(t.TextId, ArchiveDate, expanded.Content, expanded.Remarks, null);
                            }
                            await _repo.CompressVerse(t.TextId, t.timestamp, t.Content, t.Remarks, null);

                        }
                    }
                }
            }

            
		}
	}
}
