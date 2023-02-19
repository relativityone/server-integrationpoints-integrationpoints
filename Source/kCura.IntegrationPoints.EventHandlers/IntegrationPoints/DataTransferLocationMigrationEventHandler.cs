using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Description("This is an event handler to recreate Data Transfer Location directory structure after creating workspace from a template that has Integration Points installed")]
    [Guid("D9F2468C-9480-4E70-9AC7-837E2AFD3237")]
    public class DataTransferLocationMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
    {
        private IDataTransferLocationService _dataTransferLocationService;
        internal IDataTransferLocationService DataTransferLocationService
        {
            get
            {
                if (_dataTransferLocationService == null)
                {
                    ICaseServiceContext context = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
                    IIntegrationPointTypeService typeService = new IntegrationPointTypeService(Helper, context);

                    _dataTransferLocationService = new DataTransferLocationService(Helper, typeService, new LongPathDirectory(), new CryptographyHelper());
                }

                return _dataTransferLocationService;
            }
        }

        protected override void Run()
        {
            DataTransferLocationService.CreateForAllTypes(Helper.GetActiveCaseID());
        }

        protected override string SuccessMessage => "Data Transfer directories migrated successfully";
        protected override string GetFailureMessage(Exception ex) => "Failed to create Data Transfer directories";
    }
}
