using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{

	public class ExportFieldsService : IExportFieldsService
	{
		#region Fields

		private readonly IServiceManagerProvider _serviceManagerProvider;

		#endregion //Fields

		public ExportFieldsService(IServiceManagerProvider serviceManagerProvider)
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
					IsIdentifier = x.Category == FieldCategory.Identifier,
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
					IsIdentifier = x.Category == FieldCategory.Identifier,
					IsRequired = false
				}).ToArray();
		}

		public FieldEntry[] GetAllExportableLongTextFields(int workspaceArtifactID, int artifactTypeID)
		{
			ISearchManager searchManager = _serviceManagerProvider.Create<ISearchManager, SearchManagerFactory>();

			return searchManager.RetrieveAllExportableViewFields(workspaceArtifactID, artifactTypeID)
				.Where(x => x.FieldType == FieldTypeHelper.FieldType.Text || x.FieldType == FieldTypeHelper.FieldType.OffTableText)
				.Select(x => new FieldEntry
				{
					DisplayName = x.DisplayName,
					FieldIdentifier = x.AvfId.ToString(),
					FieldType = FieldType.String,
					IsIdentifier = x.Category == FieldCategory.Identifier,
					IsRequired = false
				}).ToArray();
		}

		private FieldType ConvertFromExFieldType(FieldTypeHelper.FieldType fieldType)
		{
			return fieldType == FieldTypeHelper.FieldType.File ? FieldType.File : FieldType.String;
		}
	}
}