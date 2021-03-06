﻿using System.Web.Optimization;

namespace VKAnalyzer
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            //Кастомные бандлы для спецаильных страниц

            //Cohort analyse
            bundles.Add(new ScriptBundle("~/bundles/cohortanalyse").Include(
                      "~/Scripts/jquery-ui-1.12.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/selectize").Include(

                      "~/Scripts/Plugins/selectize/selectize.js",
                      "~/Content/Plugins/selectize/selectize.css",
                      "~/Content/Plugins/selectize/selectize.bootstrap3.css"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/affinityIndex").Include(
                      "~/Scripts/jquery-ui-1.12.1.min.js",
                      "~/Scripts/Services/AffinityIndex.js"//,
                      //"~/Scripts/Plugins/selectize/selectize.js",
                      //"~/Content/Plugins/selectize/selectize.css"
                      ));
        }
    }
}
