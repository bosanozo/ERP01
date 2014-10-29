using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace WebApp
{
    public class BundleConfig
    {
        // バンドルの詳細については、http://go.microsoft.com/fwlink/?LinkId=301862 を参照してください
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            // jqueryui jqGrid
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery.jqGrid.src.js", // jqGrid
                        "~/Scripts/i18n/grid.locale-ja.js", // jqGrid 日本語メッセージ 
                        "~/Scripts/jquery.balloon.js", // バルーンチップ
                        "~/Scripts/jquery.validate-vsdoc.js", // jQuery Validation
                        "~/Scripts/localization/messages_ja.js", // jQuery Validation 日本語メッセージ
                        "~/Scripts/jquery.validate.japlugin.js" // jQuery Validation 日本語
                        ));
            
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.validation.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/sammy-{version}.js",
                "~/Scripts/app/common.js",
                "~/Scripts/app/app.datamodel.js",
                "~/Scripts/app/app.viewmodel.js",
                "~/Scripts/app/home.viewmodel.js",
                "~/Scripts/app/_run.js"));

            // 開発と学習には、Modernizr の開発バージョンを使用します。次に、実稼働の準備が
            // できたら、http://modernizr.com にあるビルド ツールを使用して、必要なテストのみを選択します。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-theme.css"));
                //"~/Content/Site.css"));

            // ローカルスタイルシート
            bundles.Add(new StyleBundle("~/Content/csslocal").Include(
                "~/Content/jquery.jqGrid/ui.jqgrid.css", // jqGrid スタイルシート
                "~/Content/SsDefaultStyle.css", // ローカルスタイルシート
                "~/Content/jqStyle.css"               
                 ));

            // デバッグを行うには EnableOptimizations を false に設定します。詳細については、
            // http://go.microsoft.com/fwlink/?LinkId=301862 を参照してください
            BundleTable.EnableOptimizations = true;
        }
    }
}
