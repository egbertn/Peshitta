using System;
using System.Net;
using System.Web;


namespace peshitta.nl
{

    /// <summary>
    /// Summary description for global
    /// </summary>
    public partial class global : HttpApplication
    {
        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            return string.IsNullOrEmpty(custom) ? null : context.Request.Headers[custom];
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            Uri orig = Request.Url;
            string host = orig.Host;
            bool islocalhost = host == "localhost" || host == "127.0.0.1" || string.Compare(host, Environment.MachineName, true) == 0;
            string origPath = orig.PathAndQuery;
            if (!host.StartsWith("www.") && !islocalhost)
            {
                Response.RedirectLocation = orig.Scheme + "://www." + host + origPath;
                Response.StatusCode = (int)HttpStatusCode.Moved;
                Response.Buffer = false;
                Response.StatusDescription = "Moved Permanently";
                return;
            }
          

           
          
       
       
           
            if (!host.EndsWith("peshitta.nl") && origPath.StartsWith("/book.aspx", StringComparison.OrdinalIgnoreCase) && !islocalhost)
            {
                var rq = Request.QueryString;
                string temp = rq["goto"];
                if (!string.IsNullOrEmpty(temp))
                {
                    temp = "&goto=" + temp;
                }
                Response.RedirectLocation = string.Format("http://www.peshitta.nl/book.aspx?bookid={0}&ch={1}", rq["bookid"], rq["ch"]) + temp;
                Response.StatusCode = (int)HttpStatusCode.Moved;
                Response.Buffer = false;
                Response.StatusDescription = "Moved Permanently";
                return;
            }
            if (!host.EndsWith("peshitta.nl") && origPath.StartsWith("/Search.aspx", StringComparison.OrdinalIgnoreCase) && !islocalhost)
            {
                Response.RedirectLocation = "http://www.peshitta.nl/Search.aspx";
                Response.StatusCode = (int)HttpStatusCode.Moved;
                Response.Buffer = false;
                Response.StatusDescription = "Moved Permanently";
                return;
            }
           
            Uri sReferrer = Request.UrlReferrer;
            if (sReferrer != null && !Request.Browser.Crawler)
            {
                string startPage = Request.PhysicalPath;
                string openingNow = orig.PathAndQuery;

                startPage = System.IO.Path.GetFileName(startPage);
                bool isAspx = startPage.EndsWith(".aspx");
                string shost = sReferrer.Host;


            }

        }
        private void Application_Start(object sender, EventArgs e)
        {
            System.Web.Http.GlobalConfiguration.Configure(Api.Config.WebApiConfig.Register);

            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            // Code that runs on application startup
            //string connString =WebConfigurationManager.ConnectionStrings["BIJBELConnectionString"].ConnectionString;

            //keepOpen = new SqlConnection(connString);
            //keepOpen.Open();
            //GlobalConfiguration.Configuration
            //    .ParameterBindingRules
            //    .Insert(0,SimplePostVariableParameterBinding.HookupParameterBinding);

            //GlobalConfiguration.Configuration.Routes.MapHttpRoute(
            //   name: "DefaultApi",
            //   routeTemplate: "{controller}/{id}",
            //   defaults: new { id = RouteParameter.Optional }
            //);
            //RouteTable.Routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Verse", action = "Index", id = UrlParameter.Optional });

            //using (var d = new BookDal())
            //{
            //    BookDal.UpdateWordsCache(d);
            //}
        }


        protected void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }


    }

}