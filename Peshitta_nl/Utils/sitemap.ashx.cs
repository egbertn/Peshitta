using System.IO;
using System;
using System.Text;
using System.Web;
using System.Linq;
using adccure;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace peshitta.nl
{
    public sealed class sitemap : IHttpHandler
    {
        //private static IEnumerable<string> GetPubs(HttpRequest request)
        //{
        //    var temp = request.QueryString["pub"];
        //    return temp.Split(',');
        //}

        private static Dictionary<string, CacheType> _cache;
        private static readonly object lockobj = new object();
        public class CacheType
        {
            public CacheType()
            {
                LastUpdate = DateTime.MinValue;
            }
            //querystring
            public byte[] Bytes { get; set; }
            //when was this URL cached?
            public DateTime LastUpdate { get; set; }
        }
        public void ProcessRequest(HttpContext context)
        {
            HttpResponse Response = context.Response;
            HttpRequest Request = context.Request;

            bool isCrawler = utils.IsCrawler(Request, out string browser);
            if (browser.Contains("Yandex") || browser.Contains("Baiduspider"))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.StatusDescription = "Unauthorized";
                return;
            }

            Response.ContentType = "text/xml";
            Response.Charset = "utf-8";

            // not needed because of BinaryWrite
            //Response.ContentEncoding = Encoding.UTF8;

            string qr = Request.QueryString.ToString();

            //var pubCodes = GetPubs(Request);
            ////TODO check on allowed pub codes, otherwise a hacker could cause memory to fill up
            //if (pubCodes.Count() > 3 || qr.Length > 20)
            //{
            //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //    Response.StatusDescription = "Invalid request";
            //    return;
            //}
            //TODO put in config file
            var formatUrl = "http://www.peshitta.nl/book.aspx?";

            //Uri uri = new Uri(formatUrl);
            //string host = uri.Host;
            //string serverHost = Request.Url.Host;
            //if (host != serverHost)
            //{
            //    Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            //    Response.StatusDescription = "Not implemented for this host";
            //    return;
            //}

            if (_cache == null)
            {
                lock (lockobj)
                {
                    _cache = new Dictionary<string, CacheType>();
                }
            }
            CacheType ct;
            if (!_cache.ContainsKey(qr))
            {
                lock (lockobj)
                {
                    ct = new CacheType();
                    //TODO check params otherwise memory migt be stuffed
                    _cache.Add(qr, ct);

                }
            }
            else
            {
                ct = _cache[qr];
            }
            if (ct.LastUpdate == DateTime.MinValue || DateTime.UtcNow - ct.LastUpdate > TimeSpan.FromHours(24))
            {
                ct.LastUpdate = DateTime.Now.AddYears(-1); //temp
                UrlSet retVal = new UrlSet();
                var dcd = utils.InstanceDBAsync().Result;
               
                var bookeditionIds = dcd.Contents.BookEditions.Values.Where(w => w.active && dcd.ActivePublications.Contains(w.publishercode));

                foreach (var bookeditionid in bookeditionIds)
                {
                  
                    var chaps = dcd.ChaptersByBookIdAsync(bookeditionid.bookid).Result;
                    foreach (var ch in chaps)
                    {
                        var chapterAlineas = ch.Values.Select(s => s.BookchapterAlineaId).ToArray();

                        var maxupd = dcd.Contents.Pubs[bookeditionid.publishercode].Texts
                            .Values.Where(w => chapterAlineas.Contains(w.BookChapterAlineaid))
                            .Max(m => m.timestamp);
                    

                        retVal.AddUrl(new Url()
                        {
                            LastModifiedDateTime = maxupd,
                            Loc = string.Format("{0}://www.peshitta.nl/book.aspx?booked={1}&ch={2}",
                                 Uri.UriSchemeHttps
                                , bookeditionid.bookEditionid, ch.Key.chapter),

                        });


                    }




                    //creates a list limited by it's existance
                    // if a bookeditioned was not published, it will not be shown.
                    using (var io = (MemoryStream)retVal.ToStream())
                    {
                        _cache[qr].Bytes = io.ToArray();
                    }
                }
            }
            string etag;
            using (var mem = new MemoryStream())
            using (var wr = new BinaryWriter(mem))
            {
                var btLen = Encoding.UTF8.GetBytes(formatUrl);
                wr.Write(btLen);
                wr.Write(ct.LastUpdate.ToBinary());
                wr.Write(_cache[qr].Bytes);
                wr.Flush();
                using (var MD5Enc = MD5.Create())
                {
                    mem.Position = 0;
                    etag = BitConverter.ToString(MD5Enc.ComputeHash(mem)).Replace("-", ""); ;
                }


                //Response.Cache.SetExpires(ct.LastUpdate.AddHours(24));
                if (utils.SetLastModified(Response, Request, ct.LastUpdate, etag))
                {
                    return;
                }
                //some old style crawlers, use HEAD instead of conditional get; so get out, and avoid memory waste.
                if (Request.HttpMethod == "HEAD")
                {
                    return;
                }

                Response.BinaryWrite(_cache[qr].Bytes);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}