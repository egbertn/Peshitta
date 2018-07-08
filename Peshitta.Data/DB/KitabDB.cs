using Newtonsoft.Json;
using Peshitta.Data.Attributes;
using Peshitta.Data.Data;
using Peshitta.Data.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Peshitta.Data.DB
{
    public partial class KitabDB
    {
        private static readonly char[] splitters = new[] { ' ', '(', ')', ';', ':','#', ',', '.','‘', '“',
                                                            '”', '&', '\'', '-', '"', '[', ']', '?', '!', '<', '>',
                                                            '=', '/', '*', '\r', '\n', '„', '—', '%', '…', '·', '´'};

        /// <summary>
        /// deals with collections in this class
        /// </summary>
        /// <returns></returns>
        private static readonly Lazy<IEnumerable<PropertyInfo>> Collections =
            new Lazy<IEnumerable<PropertyInfo>>(() =>
                typeof(KitabDB).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType.IsCollection() && p.GetGetMethod() != null));

        private static readonly ConcurrentDictionary<string, (DateTime fileTime, object data)> Cache = new ConcurrentDictionary<string, (DateTime fileTime, object data)>();
        internal KitabDB()
        {
            Contents = new All();
            _activePublications = new string[0];
            _possiblePublicatios = new string[0];//zero array
        }
        public KitabDB(All all)
        {
            this.Contents = all;
        }
        public All Contents { get; }
        private string[] _activePublications;
        /// <summary>
        /// the ones loaded from disk
        /// </summary>
        private string[] _possiblePublicatios;
        public string[] ActivePublications
        {
            get
            {
                return _activePublications;
            }
            set
            {
                //add only possible items, note, case sensitive
                _activePublications = value.Where(w => _possiblePublicatios.Contains(w)).ToArray();
            }
        }

        protected string _basePath;
        //LoadFile sets them
        protected DateTime _lastFT;


        /// <summary>
        /// Loads zz files, undeflates them, deserialises to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="Ids">if not provided, will enumerate all files, when provided, assumes files to exist and gets them directly</param>
        protected T LoadFile<T>(string path, params int[] Ids) where T : class
        {
            //it's a path, not a file, so enumerate the stuff
            if (!Path.HasExtension(path))
            {
                var files = Ids == null || Ids.Length == 0 ?
                        Directory.EnumerateFiles(Path.Combine(_basePath, path), "*.zz", SearchOption.AllDirectories)
                                            :
                        Ids.Select(s => Path.Combine(_basePath, path, $"{s}.json.zz"));


                var lst = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(typeof(T).GetGenericArguments()), new object[] { files.Count() * 20 });
                foreach (var file in files)
                {
                    foreach (var item in (IEnumerable)LoadFile<T>(file))
                    {
                        lst.Add(item);
                    }
                }
                return (T)lst;
            }
            else
            {
                T val = default(T);
                var findCacheAttribute = typeof(T).GetGenericArguments()[0];
                var mustBeCached = findCacheAttribute.IsDefined(typeof(CacheAttribute), true);
                var hasCache = false;
                var fileInfo = new FileInfo(Path.Combine(_basePath, path));
                if (mustBeCached)
                {
                    hasCache = Cache.TryGetValue(fileInfo.Name, out var vt);
                    if (hasCache && vt.fileTime == fileInfo.LastWriteTimeUtc)
                    {
                        return (T)vt.data;
                    }
                }
                if (path.EndsWith(".json.zz"))
                {                   
                    val = fileInfo.LoadUncompress<T>();

                }
                else if (path.EndsWith(".txt.zz"))
                {

                    val = fileInfo.LoadUncompressText<T>(out _lastFT);

                }
                if (mustBeCached)
                {
                    var upd = (fileInfo.LastWriteTimeUtc, val);
                    Cache.TryRemove(fileInfo.Name, out var vt);
                    Cache.TryAdd(fileInfo.Name, upd);
                }
                return val;
            }
        }
        protected T LoadFileFromHistory<T>(string path, params (int textId, DateTime ArchiveDate)[] Ids) where T : class
        {
            //it's a path, not a file, so enumerate the stuff
            if (Path.HasExtension(path))
            {
                throw new ArgumentOutOfRangeException(nameof(path), "path must not contain an extension");
            }
            var files = 
                    Ids.Select(s => Path.Combine(_basePath, path, SignificantNumber(s.textId), $"{s.textId}-{s.ArchiveDate.ToString("yyyy-MM-dd HH.mm")}.json.zz"));


            var lst = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(typeof(T).GetGenericArguments()), new object[] { files.Count() * 20 });
            foreach (var file in files)
            {
                foreach (var item in (IEnumerable)LoadFile<T>(file))
                {
                    lst.Add(item);
                }
            }
            return (T)lst;
        }
        private async Task WriteAllTextAsync(object value, string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }
            if (Path.IsPathRooted(file) || file.StartsWith("\\"))
            {
                throw new InvalidOperationException($"Path must be relative to {_basePath}");
            }
            int estimate = 4096;
            var dict = default(IDictionary);
            if (value is IDictionary)
            {
                dict = (IDictionary) value;
                if (dict.Count > 1000)
                {
                    estimate = 1000000; //bigfile                 
                }
            }
            else if (value is IEnumerable enu)
            {
                var count = 0;
                var enumerator = enu.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (count++ > 1000)
                    {
                        estimate = 1000000; //bigfile
                        break;
                    }
                }
            }
            var ser = new JsonSerializer();
            var fileS = File.Create(Path.Combine(_basePath, file), estimate, FileOptions.SequentialScan | FileOptions.Asynchronous);
            var strm = new JsonTextWriter(new StreamWriter(new DeflateStream(fileS, CompressionLevel.Fastest), Encoding.UTF8))
            {
                CloseOutput = true
            };

            ser.Serialize(strm, dict != null ? dict.Values: value);
            await strm.CloseAsync();

        }
        /// <summary>
        /// Saves the Contents of KitabDB to a path on disk using a specific directory structure
        /// Use LoadFromDisk to do the reverse
        /// </summary>
        /// <param name="baseF"></param>
        /// <returns></returns>
        public async Task BackupToDiskAsync(string baseF)
        {

            Trace.TraceInformation("Serialized {0} text items", Contents.Pubs.First().Value.Texts.Count);
            foreach (var pubCode in Contents.BookEditions.Values.Select(s => s.publishercode).Distinct())
            {
                var booked = Contents.BookEditions.Values.Where(s => s.publishercode == pubCode);

                _basePath = baseF;
                Directory.CreateDirectory(Path.Combine(_basePath, pubCode));
                await WriteAllTextAsync(Contents.Languages, "languages.json.zz").ConfigureAwait(false);
                await WriteAllTextAsync(Contents.PublicationCodes, "publicationcodes.json.zz").ConfigureAwait(false);
                await WriteAllTextAsync(Contents.Words.Values, "words.json.zz").ConfigureAwait(false);
                await WriteAllTextAsync(Contents.Books.Values, "books.json.zz").ConfigureAwait(false);
                await WriteAllTextAsync(Contents.BookChapterAlineas.Values, "bookchapteralineas.json.zz").ConfigureAwait(false);
                await WriteAllTextAsync(Contents.BookEditions.Values, "bookeditions.json.zz").ConfigureAwait(false);
                await WriteAllTextAsync(Contents.BookChapters.Values, "bookchapters.json.zz").ConfigureAwait(false);

                var p = Path.Combine(baseF, pubCode, "textwords");
                var h = Path.Combine(baseF, pubCode, "textwordshistory");
                var path = Directory.CreateDirectory(p);
                path = Directory.CreateDirectory(h);
                for (var x = 0; x < 10; x++)
                {
                    path.CreateSubdirectory(x.ToString());
                }

                var searchStream = new StreamWriter(new DeflateStream(File.Create(Path.Combine(baseF, pubCode, $"full_text.txt.zz"), 1000000, FileOptions.SequentialScan), CompressionLevel.Fastest), Encoding.UTF8);
                var data = Contents.Pubs[pubCode].Texts.Values.Where(w => booked.Select(s => s.bookEditionid).Contains(w.bookeditionid));
                await WriteAllTextAsync(data, Path.Combine(pubCode, "text.json.zz"));

                foreach (var be in booked.OrderBy(o => o.bookOrder))
                {
                    var englishBookName = be.EnglishTitle;
                    var textBuilder = new StringBuilder();

                    foreach (var text in Contents.Pubs[be.publishercode].Texts.Values.Where(t => t.bookeditionid == be.bookEditionid))
                    {
                        var wordsFromText = Contents.Pubs[be.publishercode].TextWords[text.TextId];
                        await WriteAllTextAsync(wordsFromText,
                            Path.Combine(be.publishercode, "textwords", $"{text.TextId}.json.zz")).ConfigureAwait(false);

                        /* construct a verse and append it to the searchable text */
                        var chapterId = Contents.BookChapterAlineas[new BookChapterAlineaKey(text.BookChapterAlineaid, text.Alineaid)].BookchapterId;
                        var chapter = Contents.BookChapters[chapterId].chapter;

                        var textContent = Decompress(text, wordsFromText, Contents.Words, textBuilder);

                        await searchStream.WriteLineAsync($"{englishBookName} {chapter}:{text.Alineaid} {textContent.Content}").ConfigureAwait(false);
                        /* end verse construct */

                        foreach (var histText in Contents.Pubs[be.publishercode].HistoryDates[text.TextId])
                        {
                            await Task.Run(async () =>
                            {
                                await WriteAllTextAsync(
                                Contents.Pubs[be.publishercode].TextWordsHistory[new HistoryKey(text.TextId, histText)],
                                    Path.Combine(be.publishercode, "textwordshistory", SignificantNumber(text.TextId), $"{text.TextId}-{histText.ToString("yyyy-MM-dd HH.mm")}.json.zz"));
                            }).ConfigureAwait(false);
                        }
                        //if (text.TextId % 100 == 0 && Contents.Pubs[be.publishercode].HistoryDates[text.TextId].Any())
                        //{
                        //    Trace.TraceInformation("Done {0}, {1}", text.TextId, Contents.Pubs[be.publishercode].HistoryDates[text.TextId].Last());
                        //}
                    }
                }
                await searchStream.FlushAsync().ConfigureAwait(false);
                searchStream.Close();
            }
        }
        private static PropertyInfo GetCollectionPropertyByType(IEnumerable<PropertyInfo> props, Type t)
        {
            foreach (var collection in props)
            {
                if (collection.PropertyType.GetGenericArguments()[0] == t)
                {
                    return collection;
                }
            }
            return null;
        }
        private static PropertyInfo GetIdMethod<T>(IEnumerable<T> lst)
        {
            PropertyInfo p = null;
            foreach (var findP in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (findP.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase))
                {
                    p = findP;
                    break;
                }
            }

            if (p.PropertyType != typeof(int))
            {
                throw new InvalidOperationException("Id type is not Int32, Max not supported");
            }
            return p;
        }
        private static int FindOrdinal<T>(IList<T> lst, int id)
        {
            var p = GetIdMethod(lst);
            //bool isInt = false, isGuid = false;
            //if (p.PropertyType == typeof(int))
            //{
            //    isInt = true;
            //}
            //primary keys must not be nullable
            //else if (p.PropertyType == typeof(Guid))
            //{
            //    isGuid = true;
            //}
            var enumer = lst.GetEnumerator();

            int pos = 0;
            while (enumer.MoveNext())
            {
                var idToBeFound = p.GetValue(enumer.Current);
                if (id.Equals(idToBeFound))
                {
                    return pos;
                }
                pos++;
            }
            return -1;

        }

        private static int FindOrdinal<T>(IList<T> lst, T item)
        {
            //TODO cache the type

            var p = GetIdMethod(lst);
            var idToBeFound = p.GetValue(item);
            return FindOrdinal(lst, (int)idToBeFound);
        }
        private IDictionary<int, T> GetCollectionAsDictionary<T>(PropertyInfo collectionProperty, Type t)
        {
            var theListObject = collectionProperty.GetValue(this);
            var theList = (Dictionary<int, T>)collectionProperty.GetValue(this);
            if (theList == null)
            {
                var newValue = Activator.CreateInstance<Dictionary<int, T>>();
                collectionProperty.SetValue(this, newValue);
                return newValue;
            }
            return theList;
        }
        private IList<T> GetCollection<T>(PropertyInfo collectionProperty, Type t)
        {
            var theListObject = collectionProperty.GetValue(this);
            var theList = (IList<T>)collectionProperty.GetValue(this);
            if (theList == null)
            {
                var newValue = Activator.CreateInstance<List<T>>();
                collectionProperty.SetValue(this, newValue);
                return newValue;
            }
            return theList;
        }
        //public async Task<T> SaveAsync<T>(T ob)
        //{
        //    var t = typeof(T);
        //    //TODO cache the type

        //    var collection = GetCollectionPropertyByType(Collections.Value, t);
        //    var theList = GetCollection<T>(collection, t);
        //    var maxId = 0;
        //    var idMethod = GetIdMethod(theList);
        //    if (idMethod.PropertyType == typeof(int) || idMethod.PropertyType == typeof(long))
        //    {
        //        maxId = MaxOfId(theList) + 1;
        //        idMethod.SetValue(ob, maxId);
        //    }
        //    else if (idMethod.PropertyType == typeof(Guid))
        //    {
        //        idMethod.SetValue(ob, Guid.NewGuid());
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException("No suitable primary key can be found to generate one");
        //    }

        //    theList.Add(ob);

        //    var dbAttr = t.GetCustomAttribute<DbTableAttribute>() ?? new DbTableAttribute();
        //    if (dbAttr.MemoryCache == true)
        //    {
        //        MCache.Set(collection.PropertyType, (DateTimeOffset.UtcNow, theList), TimeSpan.FromMinutes(10));
        //        // Cache.AddOrUpdate(collection.PropertyType, (DateTime.UtcNow, theList), (key, oldvalue) => (DateTime.UtcNow, theList));
        //        //bool exists = Cache.TryGetValue(t, out (DateTime, object) value);
        //        //if (exists)
        //        //{
        //        //    Cache.TryUpdate(t, (DateTime.UtcNow, theList), value);
        //        //}
        //        //else
        //        //{
        //        //    Cache.TryAdd(t, (DateTime.UtcNow, theList));
        //        //}
        //    }
        //    await WriteAllTextAsync(theList, Path.Combine(_basePath, t.Name.ToLowerInvariant()) + ".json.zz");

        //    return ob;

        //}
        public async Task<bool> DeleteAsync<T>(T obj)
        {
            var t = typeof(T);
            var collection = GetCollectionPropertyByType(Collections.Value, t);
            if (collection == null)
            {
                throw new InvalidOperationException($"Collection {t} not found");
            }
            //if (collection.)
            var collectionObj = collection.GetValue(this);
            if (collectionObj is IDictionary<int, T> theDict)
            {
                var p = typeof(T).GetProperty("id");
                var idToBeFound =(int) p.GetValue(obj);
                theDict.Remove(idToBeFound);
                await WriteAllTextAsync(theDict, Path.Combine(_basePath, t.Name.ToLowerInvariant()) + ".json.zz");
            }
            else if (collectionObj is IList<T> theList)
            {               
                var pos = FindOrdinal(theList, obj);
                if (pos >= 0)
                {
                    theList.RemoveAt(pos);
                }
                await WriteAllTextAsync(theList, Path.Combine(_basePath, t.Name.ToLowerInvariant()) + ".json.zz");
            }
           
            return true;
        }
        private void PublicationPossible(string pubCode)
        {
            if (!_possiblePublicatios.Contains(pubCode))
            {
                var curLen = _possiblePublicatios.Length;
                Array.Resize(ref _possiblePublicatios, curLen + 1);
                _possiblePublicatios[curLen] = pubCode;
            }
        }
        private IEnumerable<(int textId, DateTime ArchiveDate)> LoadHistoryDates<T>(string pubCode, params int[] textIds) where T : class
        {
            var nameofType = typeof(T).Name.ToLowerInvariant();
            foreach (var textId in textIds)
            {
                var path = Path.Combine(_basePath, pubCode, nameofType, SignificantNumber(textId));
                var info = new DirectoryInfo(path);
                var culture = CultureInfo.InvariantCulture;
                foreach (var nfo in Directory.GetFiles(path, $"{textId}-*.json.zz"))
                {
                    var file = Path.GetFileName(nfo);
                    var dash = file.IndexOf('-');
                    var date = DateTime.ParseExact(file.Substring(dash + 1, file.Length - dash - 1 - 8), "yyyy-MM-dd HH.mm", culture.DateTimeFormat);
                    yield return (textId, date);
                }
            }
        }
        public async Task<Response<bool>> CompressVerse(TextExpanded text)
        {
            var code = 204;

             if (!this.Contents.BookEditions.ContainsKey(text.bookeditionid))
            {
                code = 404;
                return new Response<bool>(1, "", DateTime.UtcNow, new[] { false }) { StatusCode = code };
            }
            var pubCode = this.Contents.BookEditions[text.bookeditionid].publishercode;

            this.ActivePublications = new[] { pubCode };
          
            var copyToHistory = this.Contents.Pubs[pubCode].TextWords == null ?
                    this.LoadFile<IEnumerable<TextWords>>(Path.Combine(pubCode, "textwords"), text.TextId).ToArray():
                    this.Contents.Pubs[pubCode].TextWords[text.TextId].ToArray();
            var now = DateTime.UtcNow;
            var oldnow = this.Contents.Pubs[pubCode].Texts[text.TextId].timestamp;
            //add to history
            await WriteAllTextAsync(copyToHistory, Path.Combine(pubCode, "textwordshistory",
                SignificantNumber(text.TextId), $"{text.TextId}-{oldnow.ToString("yyyy-MM-dd HH.mm")}.json.zz"));
            this.Contents.Pubs[pubCode].Texts[text.TextId].timestamp = now;
            var data = Contents.Pubs[pubCode].Texts.Values;
            //um, not really efficient
            await WriteAllTextAsync(data, Path.Combine(pubCode, "text.json.zz")).ConfigureAwait(false);
            var tw = CompressLine(text);
            await WriteAllTextAsync(tw, Path.Combine(pubCode, "textwords", $"{text.TextId}.json.zz"));
            return new Response<bool>(1, "", DateTime.UtcNow, new[] { true });
        }
        private IEnumerable<TextWords> CompressLine(TextExpanded t)
        {
            bool didAdd = false;
            if (t == null) return null;
            int textid = t.TextId;
            var retVal = new List<TextWords>();
            for (int x = 0; x < 3; x++)
            {
                int startAt = 0;
                int foundSplits = 0;

                // take the content text first than the footnote

                string content = null;

                switch (x)
                {
                    case 0:
                        content = t.Content;
                        break;
                    case 1:
                        content = t.Remarks;
                        break;
                    case 2:
                        content = t.Header;
                        break;
                }
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }
                int lineLen2 = content.Length;
                string theWord = null;

                bool addLBracket = false; //ch == '[';
                bool preSpace = false;
                bool addRBracket = false; //]
                bool doAddLiteral = false;
                bool addRParenthesis = false, addLParenthesis = false;
                bool addComma = false;
                bool addDot = false;
                bool addSpace = false;
                bool addHyphen = false;
                bool addEqual = false;
                bool addColon = false, addSemiColon = false;
                bool addRSQuote = false, addLSQuote = false; // '
                bool addAmp = false;
                bool addRDQuote = false, addLDQuote = false; // "
                bool addLT = false, addGT = false; // < and >
                bool addBang = false, addQMark = false;
                bool addSlash = false, addSlashAfter = false; // / for </b> for instance
                for (; ; )
                {
                    foundSplits = content.IndexOfAny(splitters, startAt);
                    // this normally happens at the very last word of a line
                    char foundChar = '\0';

                    // if a line ends without any splitter, we must take the remainer!
                    if (foundSplits < 0 && startAt < lineLen2)
                    {
                        foundSplits = lineLen2;
                    }

                    if (foundSplits < 0) break;
                    else if (foundSplits < lineLen2)
                        foundChar = content[foundSplits]; // end of line

                    int wordLen = foundSplits - startAt;
                    theWord = content.Substring(startAt, wordLen);
                    // this is the symbol which caused a split on the word

                    //there is a symbol? 
                    // deal with ( [ ' and "
                    // this section deals with symbols before any word such as ( [ - and space
                    if (wordLen == 0)
                    {
                        char ch = foundChar;
                        preSpace = ch == ' ';
                        addHyphen = ch == '-';
                        addColon = ch == ':';
                        addLBracket = ch == '[';
                        addLParenthesis = ch == '(';
                        addLSQuote = ch == '\'';
                        addBang = ch == '!';
                        addQMark = ch == '?';
                        addLDQuote = ch == '"';
                        addEqual = ch == '=';
                        addAmp = ch == '&'; //e.g. &amp; or &quote;
                        addLT = ch == '<';

                        if (addLBracket || addLParenthesis || addEqual && (startAt + 1 < lineLen2))
                        {
                            addLDQuote = content[startAt + 1] == '"';

                            if (addLDQuote && (startAt + 1 < lineLen2)) // fix for ="?booked=10" etc...
                            {
                                addQMark = content[startAt + 1] == '?';
                                if (addQMark)
                                {
                                    startAt++;
                                }
                            }
                            addLSQuote = content[startAt + 1] == '\'';
                            if (addLDQuote || addLSQuote)
                            {
                                startAt++;
                            }
                        }
                        if (addLT && startAt + 1 < lineLen2)
                        {
                            addSlash = content[startAt + 1] == '/';
                            if (addSlash) startAt++;
                        }
                        else if (startAt > lineLen2)
                        {
                            break;
                        }
                        if (addHyphen)
                        {
                            addSpace = content[startAt + 1] == ' ';
                            if (addSpace)
                            {
                                startAt++;
                            }

                        }
                        //only if we can map a char but there is no word right after (as in aller-snelste)
                        if (preSpace || addHyphen || addColon || addLBracket || addLParenthesis ||
                            addLSQuote ||
                            addLDQuote ||
                            addLT)
                        {


                            if (addHyphen)
                            {
                                //ugly workaround
                                //if the - is in the middle of a word like this
                                // He - not knowing that
                                foundChar = '-';
                                addHyphen = false;
                                doAddLiteral = true;
                            }
                            else
                            {
                                //our prefetch succeeded. Look further for / or a complete word (as in [hello] or </b)
                                startAt++;
                                continue;
                            }
                        }
                        else
                        {
                            doAddLiteral = true;
                        }
                    }
                    // this section deals with symbols after a word, such as ., ? ! : and ; etc.
                    else if (wordLen > 0)
                    {

                        char ch = foundChar; //[startAt + wordLen];
                        //var checkForSpecialWords = Contents.Words.Values.FirstOrDefault(special => special.word == theWord);
                        //if (checkForSpecialWords != null)
                        //{
                        //    // this instance must be eaten, not translated.
                        //    if (checkForSpecialWords.wordid == null)
                        //    {
                        //        if (checkForSpecialWords.specialCharacters != null)
                        //        {
                        //            while (checkForSpecialWords.specialCharacters.IndexOf(ch) >= 0)
                        //            {
                        //                ch = content[++foundSplits];
                        //            }
                        //        }
                        //        //startAt = foundSplits;
                        //        theWord = "";
                        //        goto q;
                        //    }
                        //    //TODO: fix langid

                        //    theWord = dcd.words.SingleOrDefault(a => a.id == checkForSpecialWords.wordid && a.LangId == 19 && a.IsNumber == false).word1;
                        //    if (checkForSpecialWords.CapitalizeTargetWord)
                        //    {
                        //        theWord = char.ToUpper(theWord[0]) + theWord.Substring(1);
                        //    }
                        //    if (checkForSpecialWords.specialCharacters != null)
                        //    {
                        //        while (checkForSpecialWords.specialCharacters.IndexOf(ch) >= 0)
                        //        {
                        //            ch = content[++foundSplits];
                        //        }
                        //    }
                        //}
                        q:
                        //compressing space, comma and dot delivers a compressionrate of +/- 20%                       
                        addComma = ch == ',';
                        addDot = ch == '.';
                        if (!addSpace) addSpace = ch == ' ';
                        addHyphen = ch == '-';
                        if (!addColon) addColon = ch == ':';
                        addRBracket = ch == ']';
                        addRParenthesis = ch == ')';
                        addRSQuote = ch == '\'';
                        addSlashAfter = ch == '/';
                        addRDQuote = ch == '"';
                        addGT = ch == '>';
                        addBang = ch == '!';
                        addQMark = ch == '?';
                        addSemiColon = ch == ';';
                        addEqual = ch == '=';
                        addAmp = ch == '&';
                        //addAmp = ch == '&'; //for bl=1&p=1 etc
                        bool mappedSymbol = addSemiColon || addHyphen || addBang || addRBracket || addRParenthesis || addColon ||
                              addComma || addRBracket || addDot || addGT || addRDQuote || addRSQuote || addQMark || addSlashAfter || addEqual || addAmp;

                        if (mappedSymbol || addSpace)
                            foundSplits++;


                        if (startAt + wordLen + 1 < lineLen2)
                        {
                            int lookahead = 1;
                            // we allow !" + space, ." + space, and !' + space, .' + space
                            if (addSlashAfter)
                            {
                                addGT = content[startAt + wordLen + lookahead] == '>';
                                if (addGT)
                                {
                                    lookahead++;
                                    foundSplits++;
                                }
                            }
                            if (addRSQuote) //'if ' then look for "
                            {
                                addRDQuote = content[startAt + wordLen + lookahead] == '"';
                                if (addRDQuote)
                                {
                                    lookahead++;
                                    foundSplits++;
                                }
                            }
                            if (startAt + wordLen + lookahead < lineLen2 && (addBang || addDot || addQMark || addEqual || addAmp))
                            {
                                if (!addRDQuote) addRDQuote = content[startAt + wordLen + lookahead] == '"';
                                if (!addRSQuote) addRSQuote = content[startAt + wordLen + lookahead] == '\'';

                                addRBracket = content[startAt + wordLen + lookahead] == ']';
                                addRParenthesis = content[startAt + wordLen + lookahead] == ')';

                                if (addRSQuote || addRDQuote || addRParenthesis || addRBracket)
                                {
                                    foundSplits++;
                                    lookahead++;
                                    mappedSymbol = true;
                                }
                                if (!addQMark && addRDQuote && (startAt + wordLen + lookahead < lineLen2)) // fix for ="?booked=10" etc...
                                {
                                    addQMark = content[startAt + wordLen + lookahead] == '?';
                                    if (addQMark)
                                    {
                                        foundSplits++;
                                        lookahead++;
                                        mappedSymbol = true;
                                    }
                                }
                                if (startAt + wordLen + lookahead < lineLen2 && (addRSQuote || addRDQuote))
                                {
                                    if (!addRDQuote)
                                    {
                                        addRDQuote = content[startAt + wordLen + lookahead] == '"'; // we have !'"
                                        if (addRDQuote)
                                        {
                                            foundSplits++;
                                            lookahead++;
                                        }
                                    }
                                    if (startAt + wordLen + lookahead + 1 < lineLen2)
                                    {
                                        addRBracket = content[startAt + wordLen + lookahead] == ']'; // we have !'"]
                                        if (!addGT) addGT = content[startAt + wordLen + lookahead] == '>'; // we have '">
                                        addRParenthesis = content[startAt + wordLen + lookahead] == ')';
                                        if (addRBracket || addRParenthesis || addGT)
                                        {
                                            lookahead++;
                                            foundSplits++;
                                            mappedSymbol = true;
                                        }
                                    }
                                }
                            }
                            if (mappedSymbol && !addAmp && startAt + wordLen + lookahead + 1 < lineLen2)
                            {

                                addAmp = content[startAt + wordLen + lookahead] == '&';
                                if (addAmp)
                                {
                                    lookahead++;
                                    foundSplits++;
                                }

                            }
                            if (mappedSymbol && !addSpace && startAt + wordLen + lookahead + 1 < lineLen2)
                            {

                                addSpace = content[startAt + wordLen + lookahead] == ' ';
                                if (addSpace)
                                {
                                    lookahead++;
                                    foundSplits++;
                                }

                            }
                            else if (addRSQuote || addRDQuote)
                            {
                                // we could have '"
                                if (addRSQuote)
                                {
                                    if (startAt + wordLen + lookahead + 1 < lineLen2)
                                    {
                                        addRDQuote = content[startAt + wordLen + lookahead] == '"';
                                        if (addRDQuote)
                                        {
                                            lookahead++;
                                            foundSplits++;
                                        }
                                    }
                                }
                                if (startAt + wordLen + lookahead + 1 < lineLen2)
                                {
                                    addComma = content[startAt + wordLen + lookahead] == ',';
                                    if (addComma)
                                    {
                                        lookahead++;
                                        foundSplits++;
                                    }
                                }
                                if (startAt + wordLen + lookahead + 1 < lineLen2)
                                {
                                    addSpace = content[startAt + wordLen + lookahead] == ' ';
                                    if (addSpace)
                                    {
                                        foundSplits++;
                                    }
                                }
                            }
                            // [: or (; ????
                            else if (addRBracket || addRParenthesis)
                            {
                                addColon = content[startAt + wordLen + lookahead] == ':';
                                if (addColon)
                                {
                                    mappedSymbol = true;
                                    foundSplits++;
                                }
                            }

                            //we allow symbol + space                                               
                            //if (mappedSymbol && !addSpace)
                            //{
                            //    if (startAt + wordLen + lookahead > lineLen2)
                            //    {
                            //        addSpace = content[startAt + wordLen + lookahead] == ' ';
                            //        if (addSpace)
                            //            foundSplits++;
                            //    }
                            //}
                            //we allow space + symbol
                            if (!addHyphen && addSpace)
                            {
                                addHyphen = content[startAt + wordLen + 1] == '-';
                                if (addHyphen)
                                {
                                    foundSplits++;
                                }
                            }
                        }
                        if (CompressRange(t, retVal, theWord, addSpace, addDot, addComma, x == 1, x == 2,
                            addColon, addSemiColon, addHyphen, addLBracket, addRBracket, addRParenthesis,
                            addLParenthesis, addLSQuote, addRSQuote, addLDQuote, addRDQuote, addLT, addGT, addSlash, addBang,
                            preSpace, addQMark, addSlashAfter, addEqual, addAmp))
                        {
                            didAdd = true;

                        }
                        addAmp = preSpace = addEqual = addRDQuote = addDot = addRSQuote = addRBracket =
                         addRParenthesis = addBang = addSlash =
                         addHyphen = addSemiColon = addColon =
                         addLBracket = addLParenthesis =
                         addLSQuote = addLDQuote = addSlashAfter =
                         addSpace = addGT = addLT = false;
                    }

                    //non mapped symbols go here!
                    // if the symbol also is followed by a space or dot it will be 'compressed'
                    if (doAddLiteral)
                    {
                        doAddLiteral = false;
                        char testChar = '0';

                        if (foundSplits + 1 < lineLen2)
                        {
                            testChar = content[foundSplits + 1];
                            addDot = testChar == '.';
                            addComma = testChar == ',';
                            //addGT = testChar == '>';
                            //addHyphen = testChar == '-';
                            //addRBracket = testChar == ']';
                            //addRDQuote = testChar == '"';
                            //addRSQuote = testChar == '\'';
                            //addLParenthesis = testChar == ')';
                            //addColon = testChar == ':';
                            if (!addSpace)
                            {
                                addSpace = testChar == ' ';
                            }
                            // look ahead for the case of ' or "[space]
                            // now this will be fine
                            //eg. 'Abraham', a child of God. You see, ' will be seperately encoded. 
                            // However, ' is followed by a comma and a space. The ' needs two bits set on, the comma and the space.
                            // deal with <span title="Jezus">Jeshua</span>, blah
                            if (foundSplits + 2 < lineLen2)
                            {
                                //if (addComma || addGT || addHyphen || addRBracket || addLParenthesis || addColon || addDot || addRSQuote || addRDQuote)
                                if (!addSpace)
                                {
                                    char testChar2 = content[foundSplits + 2];
                                    addSpace = testChar2 == ' ';
                                    if (addSpace)
                                        foundSplits++;
                                }

                                addRSQuote = content[foundSplits + 2] == '\'';
                                addRDQuote = content[foundSplits + 2] == '"';
                                if (addRSQuote || addRDQuote)
                                {
                                    foundSplits++;
                                }

                            }
                        }

                        if (foundChar > '\0' && CompressRange(t, retVal, new string(foundChar, 1), addSpace, addDot, addComma, x == 1, x == 2,
                            false, false,
                            false, false, false, false, false, false, addRSQuote, false, addRDQuote, false, false, false, false, false, false, false, false, false))
                        {
                          
                           
                        }
                        addRDQuote = addRSQuote = false;
                        foundSplits++;
                    }
                    startAt = foundSplits;
                }
            }
            if (didAdd)
            {
                WriteAllTextAsync(Contents.Words.Values, "words.json.zz").Wait();
            }
            return retVal;
        }
        private static void IsCapitalized(string word, out bool pIsCapitalized, out bool pIsAllCaps)
        {
            pIsCapitalized = false;
            pIsAllCaps = true;
            if (!string.IsNullOrEmpty(word))
            {
                pIsAllCaps = word.All(char.IsUpper);
                pIsCapitalized = char.IsUpper(word[0]);
                if (pIsAllCaps && word.Length == 1)
                {
                    pIsAllCaps = false; //capitalized is enough
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textid"></param>
        /// <param name="w">The single word to be converted to a number</param>
        private words FindWord(string w)
        {
            IsCapitalized(w, out bool capitalized, out bool allCaps);
            if (capitalized || allCaps)
            {
                w = w.ToLowerInvariant();
            }
            // numbers can become huge, and thus, waste space!
            bool isNumber = int.TryParse(w, out int number);
            var values = Contents.Words.Values;
            var foundWord = isNumber ? values.SingleOrDefault(a => a.number == number) : values.SingleOrDefault(a => a.word == w);
            return foundWord;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textid"></param>
        /// <param name="w">The single word to be converted to a number</param>
        private bool CompressRange(Text t, IList<TextWords> lst, string w,
            bool pAddSpace,
            bool addDot,
            bool addComma,
            bool pIsFootNote,
            bool pIsHeader,
            bool isColon,
            bool bAddSemiColon,
            bool pIsHyphen,
            bool paddLBracket,
            bool ppaddRBracket,
            bool paddRParenthesis,
            bool paddLParenthesis,
            bool paddLSQuote,
            bool paddRSQuote,
            bool paddLDQuote,
            bool paddRDQuote,
            bool paddLT,
            bool paddGT,
            bool paddSlash,
            bool paddBang, bool addPreSpace, bool pAddQMark, bool pAddSlashAfter, bool pAddEqual,
            bool pAddAmp
            )
        {
            //short wordid = 0;
            // we only find case sensitve searches
            bool didAdd = false;
            IsCapitalized(w, out bool capitalized, out bool allCaps);
            if (capitalized || allCaps)
            {
                w = w.ToLower();
            }
            // numbers can become huge, and thus, waste space!
            bool isNumber = int.TryParse(w, out int number);         
            var values = Contents.Words.Values;
            words foundWord = isNumber ? values.SingleOrDefault(f => f.number == number) : values.SingleOrDefault(f => f.word == w);

            if (foundWord == null)
            {
                foundWord = new words()
                {
                    IsNumber = isNumber,
                
                    LangId = t.langid
                };
                if (isNumber)
                {
                    foundWord.number = number;
                }
                else
                {
                    foundWord.word = w;
                }
                //gen primary key
                var maxVal = values.Max(i => i.id);
                foundWord.id = maxVal + 1;
                var dict = new Dictionary<int, words>((IDictionary<int, words>)Contents.Words)
                {
                    { foundWord.id, foundWord }
                };
                Contents.Words = dict;
                didAdd = true;
            }
            var wordid = foundWord.id;
            var tw = new TextWords()
            {
                textid = t.TextId,
                IsAllCaps = allCaps,
                AddSpace = pAddSpace,
                IsFootNote = pIsFootNote,
                IsHeader = pIsHeader,
                wordid = wordid,
                IsCapitalized = capitalized,
                AddComma = addComma,
                AddDot = addDot,
                AddColon = isColon,
                AddHyphenMin = pIsHyphen,
                LSQuote = paddLSQuote,
                RSQuote = paddRSQuote,
                RDQuote = paddRDQuote,
                LDQuote = paddLDQuote,
                RParentThesis = paddRParenthesis,
                LParentThesis = paddLParenthesis,
                AddGT = paddGT,
                AddLT = paddLT,
                AddSlash = paddSlash,
                AddBang = paddBang,
                LBracket = paddLBracket,
                // Semicolon = bAddSemiColon,
                RBracket = ppaddRBracket,
                // PreSpace = addPreSpace,
                QMark = pAddQMark,
                AddSlashAfter = pAddSlashAfter,
                AddEqual = pAddEqual,
                //  PrefixAmp = pAddAmp


            };
            //tw.word = foundWord;            
            lst.Add(tw);
            return didAdd;
        }
        public Task<Response<AlineaText>> DecompressVerse(int textId)
        {
            return LoadChapterAsync(0, 0, textId);
        }
        public Task<Response<BookEdition>> BookEditionsByPublishCode(int pBookEditionId)
        {
            var retVal = Contents.BookEditions.Values.OrderBy(o => o.bookOrder).
                Where(w => w.bookEditionid == pBookEditionId).ToArray();
            var max = retVal.Where(w => w.active).Max(m => m.PressDate ?? DateTime.MinValue);
            var eTag = CalculateHash(retVal, max);
            var resp = new Response<BookEdition>(retVal.Count(), eTag, max, retVal);
            return Task.FromResult(resp);
        }
        /// <summary>
        /// returns bookeditions queryable from current possible publications
        /// </summary>
        public Task<Response<BookEdition>> BookEditions
        {
            get
            {
                var lst = new List<BookEdition>();
                foreach (var code in _possiblePublicatios)
                {
                    var retVal = Contents.BookEditions.Values.OrderBy(o => o.bookOrder).
                        Where(w => w.publishercode == code && w.active).ToArray();
                    lst.AddRange(retVal);
                }
                var max = lst.Max(m => m.PressDate ?? DateTime.MinValue);
                var eTag = CalculateHash(lst, max);
                var resp = new Response<BookEdition>(lst.Count, eTag, max, lst);
                return Task.FromResult(resp);
            }
        }
        /// <summary>
        /// returns a tuple of Chapter and according verses (alineas)
        /// </summary>
        /// <param name="bookId"></param>
        public Task<IEnumerable<(BookChapter Key, IEnumerable<BookChapterAlinea> Values)>> ChaptersByBookEditionIdAsync(int bookEditionId)
        {
            var transIds = this.Contents.BookEditions[bookEditionId].bookid;
            var bookChapterIds = this.Contents.BookChapters.Values.Where(w => w.bookid == transIds).
                OrderBy(o => o.chapter).
                Select(s => (s, this.Contents.BookChapterAlineas.Values.Where(w => w.Equals(s))));

            return Task.FromResult(bookChapterIds);
        }
        /// <summary>
        /// returns a tuple of Chapter and according verses (alineas)
        /// </summary>
        /// <param name="bookId"></param>
        public Task<IEnumerable<(BookChapter Key, IEnumerable<BookChapterAlinea> Values)>> ChaptersByBookIdAsync(int bookId)
        {
            var bookChapterIds = this.Contents.BookChapters.Values.Where(w => w.bookid == bookId).
                OrderBy(o => o.chapter).
                Select(s => (s, this.Contents.BookChapterAlineas.Values.Where(w => w.Equals(s))));

            return Task.FromResult(bookChapterIds);
        }
        public int CompareTimeStamp(int pTextId, DateTime pWithThisTimeStamp)
        {
            foreach (var code in _activePublications)
            {
                if (this.Contents.Pubs[code].Texts.ContainsKey(pTextId))
                {
                    var text = this.Contents.Pubs[code].Texts[pTextId];
                    var tsHere = text.timestamp;
                    return tsHere.CompareTo(pWithThisTimeStamp);
                }
            }
            return -2;//non existent...
        }
        private string CalculateHash<K>(IEnumerable<K> keys, DateTime lastDate) where K : class
        {
            using (var mem = new MemoryStream())
            using (var wr = new BinaryWriter(mem))
            {
                foreach (var k in keys)
                {
                    wr.Write(k.GetHashCode());
                }
                wr.Write(lastDate.ToBinary());

                using (var MD5Enc = MD5.Create())
                {
                    mem.Position = 0;
                    return BitConverter.ToString(MD5Enc.ComputeHash(mem)).Replace("-", ""); ;
                }
            }

        }
        public Task<Response<TextExpanded>> LoadChaptersAsync(int chapter, int bookEditionId, params int[] textIdsOpt)
        {

            var lst = new List<TextExpanded>(0);
            var bookPub = Contents.BookEditions[bookEditionId];
            var pub = bookPub.publishercode;

            //convert
            // var bookeditionid = this.Contents.BookEditions.Values.FirstOrDefault(w => w.publishercode == pub && w.bookid == bookId);
            // get chapters 
            Text[] textss;
            if (textIdsOpt == null || textIdsOpt.Length == 0)
            {
                var bookChapterIds = this.Contents.BookChapters.Values.Where(w => w.bookid == bookPub.bookid && w.chapter == chapter).Select(s => s.bookchapterid).ToArray();
                var bookChapterAlineaIds = this.Contents.BookChapterAlineas.Values.
                        Where(w => bookChapterIds.Contains(w.BookchapterId)).
                        Select(s => new BookChapterAlineaKey(s.BookchapterAlineaId, s.AlineaId)).ToArray();

                //get textid's 
                textss = this.Contents.Pubs[pub].Texts.Values.
                   Where(w => bookChapterAlineaIds.Any(a => w.Equals(a))).ToArray();
            }
            else
            {
                textss = this.Contents.Pubs[pub].Texts.Values.Where(w => textIdsOpt.Contains(w.TextId)).ToArray();
            }
            var textIds = textss.Select(s => s.TextId).ToArray();

            var loadThem = LoadFile<IEnumerable<TextWords>>(Path.Combine(pub, "textwords"), textIds).ToArray();
            lst.Capacity += loadThem.Length;
            var sb = new StringBuilder();
            var grouped = loadThem.GroupBy(g => g.textid);
            var latestDT = DateTime.MinValue ;
            foreach (var t in textss)

            {
                var unexpand = Decompress(t, grouped.Where(w => w.Key == t.TextId).First(), this.Contents.Words, sb);
                lst.Add(unexpand);
                if (t.timestamp > latestDT)
                {
                    latestDT = t.timestamp;
                }
            }
       
            var eTAG = CalculateHash(lst, latestDT);
            return Task.FromResult(new Response<TextExpanded>(lst.Count, eTAG, latestDT, lst));
        }
        public IEnumerable<(DateTime ArchiveDate, TextExpanded Text)> LoadHistory(params int[] textIdsOpt)
        {
           
            var lstExpanded = new List<(DateTime, TextExpanded)>();
            foreach (var pub in _activePublications)

            {
                var dates = LoadHistoryDates<TextWordsHistory>(pub, textIdsOpt).ToArray();
                var path = Path.Combine(pub, "textwordshistory");
                var verses = LoadFileFromHistory<IEnumerable<TextWordsHistory>>(path, dates);
                var sb = new StringBuilder();
               
                foreach (var t in verses.GroupBy(t => new HistoryKey(t.textid, t.ArchiveDate)))

                {
                    var text = this.Contents.Pubs[pub].Texts[t.First().textid];
                    var unexpand = Decompress(text, t, this.Contents.Words, sb);
                    lstExpanded.Add((t.Key.ArchiveDate, unexpand));
                }
            }
            return lstExpanded;        
        }
        public Task<Response<AlineaText>> LoadChapterAsync(int chapter, int bookEditionId, params int[] textIdsOpt)
        {
            var maxV = DateTime.MinValue;
            var lst = new List<TextExpanded>(0);
            foreach (var pub in _activePublications)
            {
                //convert
                // var bookeditionid = this.Contents.BookEditions.Values.FirstOrDefault(w => w.publishercode == pub && w.bookid == bookId);
                // get chapters 
                Text[] textss;
                var bookId = this.Contents.BookEditions[bookEditionId].bookid;
                if (textIdsOpt == null || textIdsOpt.Length == 0)
                {
                    var bookChapterIds = this.Contents.BookChapters.Values.Where(w => w.bookid == bookId && w.chapter == chapter).Select(s => s.bookchapterid).ToArray();
                    var bookChapterAlineaIds = this.Contents.BookChapterAlineas.Values.
                            Where(w => bookChapterIds.Contains(w.BookchapterId)).
                            Select(s => new BookChapterAlineaKey(s.BookchapterAlineaId, s.AlineaId)).ToArray();

                    //get textid's 
                    textss = this.Contents.Pubs[pub].Texts.Values.
                       Where(w => bookChapterAlineaIds.Any(a => w.Equals(a))).ToArray();
                }
                else
                {
                    textss = this.Contents.Pubs[pub].Texts.Values.Where(w => textIdsOpt.Contains(w.TextId)).ToArray();
                }
                var textIds = textss.Select(s => s.TextId).ToArray();
                if (textIds.Length == 0)
                {
                    break;
                }
                maxV = textss.Max(m => m.timestamp);

                var loadThem = LoadFile<IEnumerable<TextWords>>(Path.Combine(pub, "textwords"), textIds).ToArray();
                lst.Capacity += loadThem.Length;
                var sb = new StringBuilder();
                var grouped = loadThem.GroupBy(g => g.textid);
                foreach (var t in textss)

                {
                    var unexpand = Decompress(t, grouped.Where(w => w.Key == t.TextId).First(), this.Contents.Words, sb);

                    lst.Add(unexpand);
                }


            };
            var eTAG = CalculateHash(lst, maxV);
            return Task.FromResult(new Response<AlineaText>(lst.Count, eTAG, maxV,
                lst.GroupBy(a => string.Format("{0:D9}{1:D3}", a.BookChapterAlineaid, a.Alineaid)).Select(t => new AlineaText()
                {
                    BookChapterAlineaId = int.Parse(t.Key.Substring(0, 9)),
                    Alineaid = int.Parse(t.Key.Substring(9, 3)),
                    Texts = t.ToList()
                })));

        }
        public static Task<KitabDB> LoadFromDiskAsync(string basePath, bool fullLoad = false)
        {

            var KitabCodes = Directory.EnumerateDirectories(basePath, "*.", SearchOption.TopDirectoryOnly);
            var retVal = new KitabDB()
            {
                _basePath = basePath
            };
            var all = retVal.Contents;
            all.PublicationCodes = retVal.LoadFile<IEnumerable<PublicationCode>>("publicationcodes.json.zz").ToArray();
            all.Languages = retVal.LoadFile<IEnumerable<Language>>("languages.json.zz").ToArray();
            all.Words = retVal.LoadFile<IEnumerable<words>>("words.json.zz").ToDictionary(a => a.id, a => a);
            all.BookChapters = retVal.LoadFile<IEnumerable<BookChapter>>("bookchapters.json.zz").ToDictionary(a => a.bookchapterid, a => a);
            all.BookChapterAlineas = retVal.LoadFile<IEnumerable<BookChapterAlinea>>("bookchapteralineas.json.zz").ToDictionary(a => new BookChapterAlineaKey(a.BookchapterAlineaId, a.AlineaId), a => a);
            all.Books = retVal.LoadFile<IEnumerable<Book>>("books.json.zz").ToDictionary(a => a.bookid, a => a);
            all.BookEditions = retVal.LoadFile<IEnumerable<BookEdition>>("bookeditions.json.zz").ToDictionary(a => a.bookEditionid, a => a);

            foreach (var code in KitabCodes.Select(Path.GetFileName))
            {
                retVal.PublicationPossible(code);

                all.Pubs.Add(code, new Publication()
                {
                    Texts = retVal.LoadFile<IEnumerable<Text>>(Path.Combine(code, "text.json.zz")).ToDictionary(a => a.TextId, a => a),
                    TextWordsHistory = fullLoad ?
                            retVal.LoadFile<IEnumerable<TextWordsHistory>>(Path.Combine(code, "textwordshistory")).ToLookup(a => new HistoryKey(a.textid, a.ArchiveDate), a => a) : null,
                    TextWords = fullLoad ?
                        retVal.LoadFile<IEnumerable<TextWords>>(Path.Combine(code, "textwords")).ToLookup(a => a.textid, a => a) : null

                });
            }
            retVal.ActivePublications = retVal._possiblePublicatios;
            return Task.FromResult(retVal);

        }
        private static bool AllOfStrings(string findStr, string line, StringComparison @case)
        {
            IEnumerable<string> all = findStr.Split(new char[] { ' ', ',', ':', '.', '-' }, StringSplitOptions.RemoveEmptyEntries);
            return all.All(a => line.IndexOf(a, @case) >= 0);
        }
        public Task<Response<string>> Search(SearchArguments args)
        {
            if (string.IsNullOrEmpty(args.FindString) || args.FindString.Length <= 2)
            {
                return Task.FromResult(new Response<string>(0, "empty", DateTime.UtcNow, new string[0]));
            }
            return Task.Run(() =>
            {
                var @case = args.ExactMatch ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
                int maxCount = args.PageSize ?? 50;
                int hitCount = 0;
                int idx = args.PageIndex ?? 0;
                int initialSkip = idx * maxCount;
                int skipC = 0;
                var lst = new List<string>(maxCount);
                //make sure we got data
                foreach (var code in _activePublications)
                {
                    if (this.Contents.Pubs[code].FullTextSearch == null)
                    {
                        this.Contents.Pubs[code].FullTextSearch = (string[])LoadFile<IEnumerable<string>>(Path.Combine(code, "full_text.txt.zz"));
                    }

                    //note: this paging trick looks nice but delivers no CPU advantage. The whole load of strings
                    // is scanned from the start, however, paging is nice for client side and reducing network traffic, which is a considerable factor for performance
                    var results = !args.ExactMatch ?
                       this.Contents.Pubs[code].FullTextSearch.Where(t =>
                           hitCount < maxCount && AllOfStrings(args.FindString, t, @case) && skipC++ >= initialSkip && hitCount++ < maxCount)
                            :
                            this.Contents.Pubs[code].FullTextSearch.Where(t => hitCount < maxCount && t.IndexOf(args.FindString, @case) >= 0 && skipC++ >= initialSkip && hitCount++ < maxCount);
                    lst.AddRange(results);
                }


                //second thought, the first search, could be stored on disk using a Guid. When paging, this could be 'looped', through if 
                // memory conserving issues are going on. The current search, is completely -inmemory-
                var eTAG = CalculateHash(lst, _lastFT);
                //TODO: lastFT oplossen is van 1 file, niet van beide
                return new Response<string>(lst.Count, eTAG, _lastFT, lst);
            });
        }
    }
}
