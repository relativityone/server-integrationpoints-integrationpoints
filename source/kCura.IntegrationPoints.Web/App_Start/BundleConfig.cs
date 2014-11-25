﻿using System.Web;
using System.Web.Optimization;

namespace kCura.IntegrationPoints.Web
{
	public class BundleConfig
	{
		// For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
									"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
									"~/Scripts/jquery-ui-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
									"~/Scripts/jquery.unobtrusive*",
									"~/Scripts/jquery.validate*"));

			bundles.Add(new ScriptBundle("~/bundles/messaging").Include(
				"~/Scripts/postal/conduit.js",
					"~/Scripts/postal/lodash.js",
					"~/Scripts/postal/postal.js",
					"~/Scripts/ip-messaging.js"
				));

			bundles.Add(new ScriptBundle("~/bundles/ipEdit").Include(
					"~/Scripts/Controls/step-progress.js",
					"~/Scripts/integration-point/edit.js",
					"~/Scripts/integration-point/step-vm.js",
					"~/Scripts/integration-point/step-details.js",
					"~/Scripts/integration-point/step-mapFields.js",
					"~/Scripts/integration-point/step-import.js"
				));

			bundles.Add(new ScriptBundle("~/bundles/dragon").Include(
				"~/Scripts/dragon/dragon-core.js",
				"~/Scripts/dragon/dragon-utils.js",
				"~/Scripts/dragon/dragon-schedule.js"
				));
			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
									"~/Scripts/modernizr-*"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
				"~/Content/normalize.css",
				"~/Content/legal-hold-fonts.css",
				"~/Content/integration-points-fonts.css",
				"~/Content/buttermilk.css",
				"~/Content/header.css",
				"~/Content/scheduler.css",
				"~/Content/select2-overrides.css",
				"~/Content/step-progress-bar.css",
				"~/Content/site.css"));

			bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
									"~/Content/themes/base/jquery.ui.core.css",
									"~/Content/themes/base/jquery.ui.resizable.css",
									"~/Content/themes/base/jquery.ui.selectable.css",
									"~/Content/themes/base/jquery.ui.accordion.css",
									"~/Content/themes/base/jquery.ui.autocomplete.css",
									"~/Content/themes/base/jquery.ui.button.css",
									"~/Content/themes/base/jquery.ui.dialog.css",
									"~/Content/themes/base/jquery.ui.slider.css",
									"~/Content/themes/base/jquery.ui.tabs.css",
									"~/Content/themes/base/jquery.ui.datepicker.css",
									"~/Content/themes/base/jquery.ui.progressbar.css",
									"~/Content/themes/base/jquery.ui.theme.css"));
		}
	}
}