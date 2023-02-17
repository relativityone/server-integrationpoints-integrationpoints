using System.Web.Optimization;

namespace kCura.IntegrationPoints.Web
{
    public static class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/node_modules/jquery/dist/jquery.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include("~/node_modules/jquery-ui/dist/jquery-ui.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*"
            ));

            bundles.Add(new ScriptBundle("~/bundles/messaging").Include(
                "~/Scripts/postal/conduit.js",
                "~/Scripts/postal/lodash.js",
                "~/Scripts/postal/postal.js",
                "~/Scripts/ip-messaging.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/frame-messaging").Include(
                "~/Scripts/jquery-postMessage.js",
                "~/Scripts/frame-messaging.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/ipEdit").Include(
                "~/Scripts/Controls/step-progress.js",
                "~/Scripts/Controls/Tooltip-ctrl.js",
                "~/Scripts/integration-point/step-definition-provider.js",
                "~/Scripts/integration-point/edit.js",
                "~/Scripts/integration-point/step-vm.js",
                "~/Scripts/integration-point/choice.js",
                "~/Scripts/integration-point/source.js",
                "~/Scripts/integration-point/destination.js",
                "~/Scripts/integration-point/profile.js",
                "~/Scripts/integration-point/scheduler.js",
                "~/Scripts/integration-point/step-details.js",
                "~/Scripts/integration-point/step-import.js ",
                "~/Scripts/integration-point/step-mapFields-control.js",
                "~/Scripts/integration-point/picker.js",
                "~/Scripts/export/export-enums.js",
                "~/Scripts/export/image-production-picker.js",
                "~/Scripts/integration-point/step-mapFields-type-validator.js",
                "~/Scripts/integration-point/step-mapFields.js",
                "~/Scripts/integration-point/tooltip-definitions.js",
                "~/Scripts/integration-point/tooltip-view-model.js",
                "~/Scripts/route.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/exportProvider").Include(
                // common
                "~/Scripts/export/export-validation.js",
                "~/Scripts/export/export-enums.js",
                "~/Scripts/export/export-helpers.js",
                // fields selection
                "~/Scripts/export/saved-search-service.js",
                "~/Scripts/export/saved-search-picker.js",
                "~/Scripts/export/export-provider-fields-step.js",
                // settings
                "~/Scripts/Export/field-mapping-view-model.js",
                "~/Scripts/export/list-picker-view-model.js",
                "~/Scripts/export/export-source-view-model.js",
                "~/Scripts/export/text-precedence-picker.js",
                "~/Scripts/export/image-production-picker.js",
                "~/Scripts/export/location-jstree-selector.js",
                "~/Scripts/export/export-provider-settings-step.js",
                "~/Scripts/export/export-provider-file-name-vm.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/importProvider").Include(
                "~/Scripts/Import/import-init.js",
                "~/Scripts/Import/import-model.js",
                "~/Scripts/Import/import-setup.js",
                "~/Scripts/Import/import-provider-settings-step.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/grid").Include(
                "~/Scripts/grid/dragon-utils.js",
                "~/Scripts/i18n/grid.locale-en.js",
                "~/node_modules/jqGrid/js/jquery.jqGrid.min.js",
                "~/Scripts/select2.min.js",
                "~/Scripts/grid/dragon-grid.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/dragon").Include(
                "~/Scripts/dragon/dragon-core.js",
                "~/Scripts/dragon/dragon-utils.js",
                "~/Scripts/dragon/dragon-schedule.js",
                "~/Scripts/dragon/dragon-dialogs.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/modals")
                .IncludeDirectory("~/Scripts/modals", "*.js", true));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/styles").Include(
                "~/Content/normalize.css",
                "~/Content/legal-hold-fonts.css",
                "~/Content/header.css",
                "~/Content/scheduler.css",
                "~/Content/site.css",
                "~/Content/step-progress-bar.css",
                "~/Content/select2.css",
                "~/Content/select2-overrides.css",
                "~/Content/select2search.css",
                "~/Content/buttermilk.css"
            ));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                "~/Content/themes/base/core.css",
                "~/Content/themes/base/resizable.css",
                "~/Content/themes/base/selectable.css",
                "~/Content/themes/base/accordion.css",
                "~/Content/themes/base/autocomplete.css",
                "~/Content/themes/base/button.css",
                "~/Content/themes/base/dialog.css",
                "~/Content/themes/base/slider.css",
                "~/Content/themes/base/tabs.css",
                "~/Content/themes/base/datepicker.css",
                "~/Content/themes/base/progressbar.css",
                "~/Content/themes/base/theme.css",
                "~/node_modules/jqGrid/css/ui.jqgrid.css"
            ));

            bundles.Add(new StyleBundle("~/Content/fonts/bundleFonts").Include(
                "~/Content/fonts/open-sans-v15-latin-regular.css"
            ));
        }
    }
}
