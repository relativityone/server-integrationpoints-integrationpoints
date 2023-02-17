using System.Collections.Generic;
using System.Text;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
    public class CommonScripts : ICommonScripts
    {
        protected readonly IScriptsHelper ScriptsHelper;
        private readonly IIntegrationPointBaseFieldGuidsConstants _guidsConstants;

        public CommonScripts(IScriptsHelper scriptsHelper, IIntegrationPointBaseFieldGuidsConstants guidsConstants)
        {
            ScriptsHelper = scriptsHelper;
            _guidsConstants = guidsConstants;
        }

        public virtual IList<string> LinkedCss()
        {
            return new List<string>
            {
                "/Content/jquery.jqGrid/ui.jqgrid.css",
                "/Content/legal-hold-fonts.css",
                "/Content/Site.css",
                "/Content/controls.grid.css",
                "/Content/controls-grid-pager.css",
                "/Content/buttermilk.css",
                "/Content/save-profile-modal.css"
            };
        }

        public virtual IList<string> LinkedScripts()
        {
            var scripts = new List<string>
            {
                "/Scripts/knockout-3.5.1.js",
                "/Scripts/knockout.validation.js",
                "/Scripts/route.js",
                "/Scripts/date.js",
                "/Scripts/q.js",
                "/Scripts/core/messaging.js",
                "/Scripts/loading-modal.js",
                "/Scripts/dragon/dragon-dialogs.js",
                "/Scripts/core/data.js",
                "/Scripts/core/utils.js",
                "/Scripts/integration-point/time-utils.js",
                "/Scripts/integration-point/picker.js",
                "/Scripts/Export/export-validation.js",
                "/Scripts/integration-point/save-as-profile-modal-vm.js"
            };

            if (IsIntegrationPointPage())
            {
                scripts.AddRange(new[]
                {
                    "/Scripts/jquery.signalR-2.3.0.js",
                    "/signalr/hubs",
                    "/Scripts/hubs/integrationPointHub.js"
                });
            }

            scripts.AddRange(new[]
            {
                "/Scripts/EventHandlers/integration-points-view-destination.js",
                "/Scripts/EventHandlers/integration-points-summary-page-view.js"
            });

            return scripts;
        }

        public virtual IList<string> ScriptBlocks()
        {
            return new List<string>
            {
                GenerateIPModel()
            };
        }

        private string GenerateIPModel()
        {
            int applicationId = ScriptsHelper.GetApplicationId();
            int activeArtifactId = ScriptsHelper.GetActiveArtifactId();
            string applicationPath = ScriptsHelper.GetApplicationPath();
            string sourceViewUrl = ScriptsHelper.GetSourceViewUrl();
            string apiControllerName = ScriptsHelper.GetAPIControllerName();

            int nextScheduledRuntimeFieldId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.NextScheduledRuntimeUTC);
            int destinationFieldId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.DestinationConfiguration);
            int destinationProviderFieldId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.DestinationProvider);
            int sourceProviderFieldId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.SourceProvider);
            int lastRuntimeFieldId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.LastRuntimeUTC);
            int sourceConfigurationFieldId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.SourceConfiguration);
            int overwriteFieldsId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.OverwriteFields);
            int hasErrorsId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.HasErrors);
            int logErrorsId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.LogErrors);
            int nameId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.Name);
            int emailNotificationId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.EmailNotificationRecipients);
            int scheduleRuleId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.ScheduleRule);
            int promoteEligibleId = ScriptsHelper.GetArtifactIdByGuid(_guidsConstants.PromoteEligible);

            StringBuilder ipModelBuilder = new StringBuilder();
            ipModelBuilder.AppendLine("<script>");

            ipModelBuilder.AppendLine("var IP = IP ||{};");

            ipModelBuilder.AppendLine($"IP.nextTimeid = ['{nextScheduledRuntimeFieldId}', '{lastRuntimeFieldId}'];");

            ipModelBuilder.AppendLine(FormatIPProperty("cpPath", applicationPath));
            ipModelBuilder.AppendLine(FormatIPProperty("destinationid", destinationFieldId));
            ipModelBuilder.AppendLine(FormatIPProperty("destinationProviderid", destinationProviderFieldId));
            ipModelBuilder.AppendLine(FormatIPProperty("sourceProviderId", sourceProviderFieldId));
            ipModelBuilder.AppendLine(FormatIPProperty("artifactid", activeArtifactId));
            ipModelBuilder.AppendLine(FormatIPProperty("appid", applicationId));
            ipModelBuilder.AppendLine(FormatIPProperty("sourceConfiguration", sourceConfigurationFieldId));
            ipModelBuilder.AppendLine(FormatIPProperty("overwriteFieldsId", overwriteFieldsId));
            ipModelBuilder.AppendLine(FormatIPProperty("hasErrorsId", hasErrorsId));
            ipModelBuilder.AppendLine(FormatIPProperty("logErrorsId", logErrorsId));
            ipModelBuilder.AppendLine(FormatIPProperty("nameId", nameId));
            ipModelBuilder.AppendLine(FormatIPProperty("emailNotificationId", emailNotificationId));
            ipModelBuilder.AppendLine(FormatIPProperty("promoteEligibleId", promoteEligibleId));
            ipModelBuilder.AppendLine(FormatIPProperty("apiControllerName", apiControllerName));

            ipModelBuilder.AppendLine("IP.params = IP.params || {};");
            ipModelBuilder.AppendLine($"IP.params['scheduleRuleId'] = '{scheduleRuleId}';");
            ipModelBuilder.AppendLine($"IP.params['sourceUrl'] = '{sourceViewUrl}';");

            ipModelBuilder.AppendLine("</script>");

            return ipModelBuilder.ToString();
        }

        private string FormatIPProperty(string name, object value)
        {
            return string.Format($"IP.{name} = '{value}';");
        }

        private bool IsIntegrationPointPage() =>
            ScriptsHelper.GetAPIControllerName() == "IntegrationPointsAPI";
    }
}
