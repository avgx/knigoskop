using System.Web.Optimization;

namespace Knigoskop.Site
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-1.*"
                            ));
            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                 "~/Scripts/knockout*"
                             ));
            bundles.Add(new ScriptBundle("~/bundles/truncate").Include(
                "~/Scripts/jquery.truncate*"
                            ));
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery-ui*"
                            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.uniform*",
                "~/Scripts/jquery.validate.*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"
                            ));

            bundles.Add(new StyleBundle("~/content/css").Include(
                "~/Content/style.css",
                "~/Content/themes/base/jquery*"
                            ));
        }
    }
}