using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Description("This is an event handler to register back integration point types after creating workspace using the template that has integration point installed.")]
    [Guid("BC67BB8A-5C10-4559-A7CF-765556BD5748")]
    public class IntegrationPointTypesMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
    {
        private IIntegrationPointTypeInstaller _integrationPointTypeInstaller;

        internal IIntegrationPointTypeInstaller IntegrationPointTypeInstaller
        {
            get
            {
                if (_integrationPointTypeInstaller == null)
                {
                    ICaseServiceContext caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
                    var typeService = new IntegrationPointTypeService(Helper, caseServiceContext);
                    _integrationPointTypeInstaller = new IntegrationPointTypeInstaller(caseServiceContext, typeService, Logger);
                }
                return _integrationPointTypeInstaller;
            }
            set { _integrationPointTypeInstaller = value; }
        }

        protected override void Run()
        {
            List<IntegrationPointType> integrationPointTypes = GetExistingIntegrationPointTypes();

            Dictionary<Guid, string> types = integrationPointTypes.ToDictionary(x => new Guid(x.Identifier), y => y.Name);
            IntegrationPointTypeInstaller.Install(types);
        }

        protected override string SuccessMessage => "Types migrated successfully.";

        protected override string GetFailureMessage(Exception ex) => "Failed to migrate Integration Point Types.";

        private List<IntegrationPointType> GetExistingIntegrationPointTypes()
        {
            var query = new QueryRequest { Fields = GetAllIntegrationPointTypeFields() };
            return WorkspaceTemplateServiceContext.RelativityObjectManagerService.RelativityObjectManager.Query<IntegrationPointType>(query);
        }

        private List<FieldRef> GetAllIntegrationPointTypeFields()
        {
            return BaseRdo.GetFieldMetadata(typeof(IntegrationPointType)).Select(pair => new FieldRef { Guid = pair.Value.FieldGuid }).ToList();
        }
    }
}