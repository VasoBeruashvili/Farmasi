using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Farmasi
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["Language"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(cookie.Value);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cookie.Value);

                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cookie.Value);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(cookie.Value);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("ka-GE");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("ka-GE");

                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ka-GE");
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ka-GE");
            }
        }
    }
}
