using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to recreate Data Transfer Location directory structure after creating workspace from a template that has Integration Points installed")]
	[Guid("D9F2468C-9480-4E70-9AC7-837E2AFD3237")]
	public class DataTransferLocationMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private IAPILog _logger;
		private IDataTransferLocationService _dataTransferLocationService;

		internal IAPILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = Helper.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationMigrationEventHandler>();
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

					_dataTransferLocationService = new DataTransferLocationService(Helper, typeService, new LongPathDirectory(), null, null);
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
					Message = "Data Transfer directories migrated successfully",
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