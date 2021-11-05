using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{

	public class WebAPIExportFieldsService : IExportFieldsService
	{
		#region Fields

		private readonly IServiceManagerProvider _serviceManagerProvider;

		#endregion //Fields

		public WebAPIExportFieldsService(IServiceManagerProvider serviceManagerProvider)
		{
			_serviceManagerProvider = serviceManagerProvider;
		}

		public FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID)
		{
			ISearchManager searchManager = _serviceManagerProvider.Create<ISearchManager, SearchManagerFactory>();

			return searchManager.RetrieveAllExportableViewFields(workspaceArtifactID, artifactTypeID)
				.Select(x => new FieldEntry
				{
					DisplayName = x.DisplayName,
					FieldIdentifier = x.AvfId.ToString(),
					FieldType = ConvertFromExFieldType(x.FieldType),
					IsIdentifier = x.Category == global::Relativity.DataExchange.Service.FieldCategory.Identifier,
					IsRequired = false
				}).ToArray();
		}

		public FieldEntry[] GetDefaultViewFields(int workspaceArtifactID, int viewArtifactID, int artifactTypeID, bool isProduction)
		{
			ISearchManager searchManager = _serviceManagerProvider.Create<ISearchManager, SearchManagerFactory>();

			IEnumerable<int> viewFieldIds = searchManager.RetrieveDefaultViewFieldIds(workspaceArtifactID, viewArtifactID, artifactTypeID, isProduction);

			return searchManager.RetrieveAllExportableViewFields(workspaceArtifactID, artifactTypeID)
				.Where(x => viewFieldIds.Contains(x.AvfId))
				.Select(x => new FieldEntry
				{
					DisplayName = x.DisplayName,
					FieldIdentifier = x.AvfId.ToString(),
					FieldType = ConvertFromExFieldType(x.FieldType),
					IsIdentifier = x.Category == global::Relativity.DataExchange.Service.FieldCategory.Identifier,
					IsRequired = false
				}).ToArray();
		}

		public FieldEntry[] GetAllExportableLongTextFields(int workspaceArtifactID, int artifactTypeID)
		{
			ISearchManager searchManager = _serviceManagerProvider.Create<ISearchManager, SearchManagerFactory>();
			return searchManager.RetrieveAllExportableViewFields(workspaceArtifactID, artifactTypeID)
				.Where(x => x.FieldType == global::Relativity.DataExchange.Service.FieldType.Text || x.FieldType == global::Relativity.DataExchange.Service.FieldType.OffTableText)
				.Select(x => new FieldEntry
				{
					DisplayName = x.DisplayName,
					FieldIdentifier = x.AvfId.ToString(),
					FieldType = FieldType.String,
					IsIdentifier = x.Category == global::Relativity.DataExchange.Service.FieldCategory.Identifier,
					IsRequired = false
				}).ToArray();
		}

		public ViewFieldInfo[] RetrieveAllExportableViewFields(int workspaceID, int artifactTypeID, string correlationID)
		{
			ISearchManager searchManager = _serviceManagerProvider.Create<ISearchManager, SearchManagerFactory>();
			
			return searchManager.RetrieveAllExportableViewFields(workspaceID, artifactTypeID);
		}

		private FieldType ConvertFromExFieldType(global::Relativity.DataExchange.Service.FieldType fieldType)
		{
			return fieldType == global::Relativity.DataExchange.Service.FieldType.File ? FieldType.File : FieldType.String;
		}
	}
}