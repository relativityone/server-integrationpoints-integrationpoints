using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Add Integration Points Types into RIP application.")]
    [RunOnce(true)]
    [Guid("28D0FB0A-3CE9-44A3-9774-3CCC9DD57870")]
    public class RegisterIntegrationPointTypeInstaller : PostInstallEventHandlerBase
    {
        private IIntegrationPointTypeInstaller _integrationPointTypeInstaller;

        internal IIntegrationPointTypeInstaller IntegrationPointTypeInstaller
        {
            get
            {
                ICaseServiceContext caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
                var typeService = new IntegrationPointTypeService(Helper, caseServiceContext);
                return _integrationPointTypeInstaller ?? (_integrationPointTypeInstaller = new IntegrationPointTypeInstaller(caseServiceContext, typeService, Logger));
            }
        }

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<RegisterIntegrationPointTypeInstaller>();
        }

        protected override string SuccessMessage => "Registration Import/Export GUIDs in database (IntegrationPointType table) completed successfully.";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Registration Import/Export GUIDs in database (IntegrationPointType table) failed.";
        }

        protected override void Run()
        {
            var types = new Dictionary<Guid, string>
            {
                [Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid] = Constants.IntegrationPoints.IntegrationPointTypes.ImportName,
                [Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid] = Constants.IntegrationPoints.IntegrationPointTypes.ExportName
            };
            IntegrationPointTypeInstaller.Install(types);
        }
    }
}