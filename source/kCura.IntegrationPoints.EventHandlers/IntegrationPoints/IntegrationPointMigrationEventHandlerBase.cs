using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public abstract class IntegrationPointMigrationEventHandlerBase : PostWorkspaceCreateEventHandlerBase
	{
		private ICaseServiceContext _workspaceTemplateServiceContext;

		protected ICaseServiceContext WorkspaceTemplateServiceContext
		{
			get
			{
				if (_workspaceTemplateServiceContext == null)
				{
					_workspaceTemplateServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, TemplateWorkspaceID);
				}
				return _workspaceTemplateServiceContext;
			}
		}

		protected virtual List<SourceProvider> GetSourceProvidersFromPreviousWorkspace()
		{
			Query<RDO> query = new Query<RDO>();
			query.Fields = GetAllSourceProviderFields();

			List<Data.SourceProvider> sourceProviderRdos = WorkspaceTemplateServiceContext.RsapiService.SourceProviderLibrary.Query(query);

			return sourceProviderRdos ?? new List<SourceProvider>();
		}

		private List<Relativity.Client.DTOs.FieldValue> GetAllSourceProviderFields()
		{
			List<Relativity.Client.DTOs.FieldValue> fields = BaseRdo.GetFieldMetadata(typeof (Data.SourceProvider)).Select(pair => new Relativity.Client.DTOs.FieldValue(pair.Value.FieldGuid)).ToList();
			return fields;
		} 

	}
}