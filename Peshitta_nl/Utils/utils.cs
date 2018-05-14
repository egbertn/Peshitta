using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using System.Net.Configuration;
using System.Web.Configuration;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Globalization;
using Peshitta.Data.DB;
using System.Web.Hosting;
using System.Threading.Tasks;

namespace peshitta.nl
{
    public static class utils
    {
        public async static Task<KitabDB> InstanceDBAsync()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/Bijbel");

            var kitab = await KitabDB.LoadFromDiskAsync(path);
            return kitab;
        }
        public static T? BoolToNullable<T>(this T value) where T : struct
        {
            if (value is bool && value.Equals( false))
            {
            
                return  default(T?);
            }
            else if (value is int && value.Equals(0))
            {
                return default(T?);
            }
            return (T)Convert.ChangeType(value, typeof(T));
           
        }
        public static bool IsCrawler(HttpRequest request, out string which)
        {
            // set next line to "bool isCrawler = false; to use this to deny certain bots 

            var browser = request.Browser;
            which = browser.Browser;
            bool isCrawler = browser.Crawler;
            // Microsoft doesn't properly detect several crawlers 
            if (!isCrawler)
            {
                // put any additional known crawlers in the Regex below 
                // you can also use this list to deny certain bots instead, if desired: 
                // just set bool isCrawler = false; for first line in method  
                // and only have the ones you want to deny in the following Regex list 
                Regex regEx = new Regex("Slurp|ask|Teoma", RegexOptions.IgnoreCase);
                isCrawler = regEx.Match(which).Success;
            }
            return isCrawler;
        }
        public static bool IsCrawler(HttpRequest request)
        {
            return IsCrawler(request, out string blah);
        }
        public static byte[] GetNetworkByteAddress(string host)
        {
            bool succes = IPAddress.TryParse(host, out IPAddress ipadress);
            if (!succes) return new byte[0];
            //int arrLen = copy.Length;
            bool bIsIP6 = ipadress.AddressFamily == AddressFamily.InterNetworkV6;
            if (!bIsIP6)
            {
                byte[] bts = ipadress.GetAddressBytes();
                Array.Resize(ref bts, 4);
                return bts;
            }
            else
            {
                return ipadress.GetAddressBytes();
            }
        }
        public static string GMT(DateTime dt)
        {
            var cultinfo = CultureInfo.GetCultureInfo(1033);
            return dt.ToString(cultinfo.DateTimeFormat.RFC1123Pattern, cultinfo);
        }
        public static bool SetLastModified(HttpResponse Response, HttpRequest Request, DateTime dateTime, string pETAG)
        {
            //avoid argument out of range exception
            //if (dateTime > DateTime.Now)
            //{
            //    dateTime = DateTime.Now;
            //}

            Response.Cache.SetLastModified(dateTime);
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetMaxAge(TimeSpan.FromDays(7));
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(7));
            Response.Cache.SetETag(pETAG);
            //default is 'private' for ASP.NET is something that searchengines do not like
            //	If-None-Match and ETag form a pair
            string eTag = Request.Headers["If-Modified-Since"];
            string realETag = Request.Headers["If-None-Match"];

            if (!string.IsNullOrEmpty(eTag))
            {

                var cultinfo = CultureInfo.GetCultureInfo(1033);
                bool result = DateTime.TryParseExact(eTag, cultinfo.DateTimeFormat.RFC1123Pattern, cultinfo, DateTimeStyles.AssumeUniversal, out DateTime dtModifiedSince);
                if (result == false) return false;
                TimeSpan ModifyDiff = dateTime - dtModifiedSince;
                if (ModifyDiff <= TimeSpan.FromSeconds(1) && realETag == pETAG)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotModified;
                    Response.StatusDescription = "Not modified";
                    Response.Buffer = false;
                    //Response.ClearContent();                
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// the database defines the last modified verse this makes the document version!
        /// Helps search engines to like our website, some even do not index dynamic sites
        /// </summary>
        /// <param name="dateTime"></param>
        public static bool SetLastModified(Page page, DateTime dateTime, string pETAG)
        {
            Literal lblUpdate = (Literal)page.FindControl("lblUpdate");
            if (lblUpdate == null && page.Master != null)
            {
                lblUpdate = (Literal)page.Master.FindControl("lblUpdate");
            }
            lblUpdate.Text = dateTime.ToLongDateString();
            return SetLastModified(page.Response, page.Request, dateTime, pETAG);
        }
        //public static void doRedir(Page page, int bookEdid)
        //{
        //    HttpResponse Response = page.Response;
        //    string jump = page.Request.QueryString["goto"];
        //    if (!string.IsNullOrEmpty(jump))
        //    {
        //        bool isNum = int.TryParse(jump, out int result);
        //        if (isNum)
        //        {
        //            using (var dcd = new Entities())
        //            {
        //                var q = from t in dcd.Texts
        //                        where t.textid == result
        //                        select t.BookChapterAlineaid;
        //                int? resultalineaId = q.SingleOrDefault();
        //                if (resultalineaId != null)
        //                {
        //                    jump = resultalineaId.Value.ToString();
        //                }
        //            }
        //        }
        //    }
        //    Response.StatusCode = (int)HttpStatusCode.Moved;
        //    Response.StatusDescription = "Moved Permanently";
        //    Response.RedirectLocation = string.Format("http://www.peshitta.nl/book.aspx?bookid={0}&ch=1", bookEdid) + jump;
        //    Response.End();
        //    return;
        //}
        static public void LogByMail(string subject, string body)
        {

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
                return;
            Configuration configurationFile = WebConfigurationManager.OpenWebConfiguration("~/Web.config");
            AppSettingsSection appSettings = configurationFile.AppSettings;
            MailSettingsSectionGroup mailSettings = configurationFile.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
            //TODO split if more mails are found
            string notiMail = appSettings.Settings["salesNotificationMails"].Value;
            string noReply = mailSettings.Smtp.From;
       
            SmtpClient mailClient = new SmtpClient();
            mailClient.Send(new MailMessage(noReply, notiMail, subject, body)
            {
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = false
            });
        }

    }
}