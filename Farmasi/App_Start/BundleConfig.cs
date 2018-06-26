using System.Web;
using System.Web.Optimization;

namespace Farmasi
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.10.2.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-2.6.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/styles.css",
                      "~/Content/font-awesome.min.css"));

            bundles.Add(new StyleBundle("~/Scripts/slick/slick-slider-css").Include(
                      "~/Scripts/slick/slick.css",
                      "~/Scripts/slick/slick-theme.css"));

            bundles.Add(new ScriptBundle("~/Scripts/slick/slick-slider-js").Include(
                      "~/Scripts/slick/slick.min.js",
                      "~/Scripts/slick-starter.js"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                      "~/Scripts/angular.min.js",
                      "~/Scripts/moment.min.js",
                      "~/Scripts/ui-bootstrap-tpls-0.13.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/global").Include(
                      "~/Scripts/custom-functions.js",
                      "~/Scripts/farmasi/global.js",
                      "~/Scripts/farmasi/services/general-service.js"));

            bundles.Add(new StyleBundle("~/Scripts/cloud-zoom/cloud-zoom-css").Include(
                      "~/Scripts/cloud-zoom/cloud-zoom.css"));

            bundles.Add(new ScriptBundle("~/Scripts/cloud-zoom/cloud-zoom-js").Include(
                      "~/Scripts/cloud-zoom/cloud-zoom.1.0.2.min.js"));
        }
    }
}
