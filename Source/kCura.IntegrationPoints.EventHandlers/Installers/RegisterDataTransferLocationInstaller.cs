using System;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Create Data Transfer folders structure")]
	[RunOnce(false)]
	[Guid("F391252A-FD72-4EF4-B323-650C70A66B81")]
	public class RegisterDataTransferLocationInstaller : PostInstallEventHandler
	{
		private IAPILog _logger;
		private IDataTransferLocationService _dataTransferLocationService;

		internal IAPILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = Helper.GetLoggerFactory().GetLogger().ForContext<RegisterDataTransferLocationInstaller>();
				}

				return _logger;
			}
		}

		internal IDataTransferLocationService DataTransferLocationService
		{
			get
			{
				if (_dataTransferLocationService == null)
				{
					ICaseServiceContext context = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
					IIntegrationPointTypeService typeService = new IntegrationPointTypeService(Helper, context);

					_dataTransferLocationService = new DataTransferLocationService(Helper, typeService);
				}

				return _dataTransferLocationService;
			}
		}

		public override Response Execute()
		{
			try
			{				
				DataTransferLocationService.CreateForAllTypes(Helper.GetActiveCaseID());

				return new Response
				{
					Message = "Data Transfer directories created successfully",
					Success = true
				};
			}
			catch (Exception ex)
			{
				LogCreatingDataTransferLocationError(ex);

				return new Response
				{
					Exception = ex,
					Message = ex.Message,
					Success = false
				};
			}
		}

		#region Logging

		private void LogCreatingDataTransferLocationError(Exception exception)
		{
			Logger.LogError(exception, "Failed to create Data Transfer directories");
		}

		#endregion
	}
}