using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using FieldValue = kCura.Relativity.Client.DTOs.FieldValue;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to register back integration point types after creating workspace using the template that has integration point installed.")]
	[Guid("BC67BB8A-5C10-4559-A7CF-765556BD5748")]
	public class IntegrationPointTypesMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private IAPILog _logger;
		private IIntegrationPointTypeInstaller _integrationPointTypeInstaller;


		internal IAPILog Logger
		{
			get { return _logger ?? (_logger = Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointTypesMigrationEventHandler>()); }
			set { _logger = value; }
		}

		internal IIntegrationPointTypeInstaller IntegrationPointTypeInstaller
		{
			get
			{
				if (_integrationPointTypeInstaller == null)
				{
					var caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
					var typeService = new IntegrationPointTypeService(Helper, caseServiceContext);
					_integrationPointTypeInstaller = new IntegrationPointTypeInstaller(caseServiceContext, typeService, Logger);
				}
				return _integrationPointTypeInstaller;
			}
			set { _integrationPointTypeInstaller = value; }
		}

		public override Response Execute()
		{
			try
			{
				var integrationPointTypes = GetExistingIntegrationPointTypes();

				Dictionary<Guid, string> types = integrationPointTypes.ToDictionary(x => new Guid(x.Identifier), y => y.Name);
				IntegrationPointTypeInstaller.Install(types);
			}
			catch (Exception e)
			{
				LogMigratingIntegrationPointTypesError(e);
				return new Response
				{
					Exception = e,
					Message = e.Message,
					Success = false
				};
			}
			return new Response
			{
				Message = "Types migrated successfully.",
				Success = true
			};
		}

		private List<IntegrationPointType> GetExistingIntegrationPointTypes()
		{
			Query<RDO> query = new Query<RDO> {Fields = GetAllIntegrationPointTypeFields()};
			return WorkspaceTemplateServiceContext.RsapiService.IntegrationPointTypeLibrary.Query(query);
		}

		private List<FieldValue> GetAllIntegrationPointTypeFields()
		{
			return BaseRdo.GetFieldMetadata(typeof(IntegrationPointType)).Select(pair => new FieldValue(pair.Value.FieldGuid)).ToList();
		}

		#region Logging

		private void LogMigratingIntegrationPointTypesError(Exception e)
		{
			Logger.LogError(e, "Failed to migrate Integration Point Types.");
		}

		#endregion
	}
}