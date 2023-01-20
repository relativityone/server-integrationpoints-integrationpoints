using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Create Data Transfer folders structure")]
    [RunOnce(true)]
    [Guid("F391252A-FD72-4EF4-B323-650C70A66B81")]
    public class RegisterDataTransferLocationInstaller : PostInstallEventHandlerBase
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

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<RegisterDataTransferLocationInstaller>();
        }

        protected override string SuccessMessage => "Data Transfer directories created successfully";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Failed to create Data Transfer directories";
        }

        protected override void Run()
        {
            int wkspId = Helper.GetActiveCaseID();
            Logger.LogInformation("Start creating data transfer folders creation for wksp...{wkspId}", wkspId);
            DataTransferLocationService.CreateForAllTypes(Helper.GetActiveCaseID());
        }
    }
}