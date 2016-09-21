using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{

	public class ExportFieldsService : IExportFieldsService
	{
		private readonly IConfig _config;
		private readonly ICredentialProvider _credentialProvider;

		public ExportFieldsService(IConfig config, ICredentialProvider credentialProvider)
		{
			_config = config;
			_credentialProvider = credentialProvider;
		}

		public FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID)
		{
			ISearchManager searchManager = ServiceManagerProvider.Create<ISearchManager, SearchManagerFactory>(_config, _credentialProvider);

			return searchManager.RetrieveAllExportableViewFields(workspaceArtifactID, artifactTypeID)
				.Select(x => new FieldEntry
				{
					DisplayName = x.DisplayName,
					FieldIdentifier = x.AvfId.ToString(),
					FieldType = FieldType.String,
					IsIdentifier = x.Category == FieldCategory.Identifier,
					IsRequired = false
				}).ToArray();
		}

		public FieldEntry[] GetDefaultViewFields(int workspaceArtifactID, int viewArtifactID, int artifactTypeID, bool isProduction)
		{
			ISearchManager searchManager = ServiceManagerProvider.Create<ISearchManager, SearchManagerFactory>(_config, _credentialProvider);

			IEnumerable<int> viewFieldIds = searchManager.RetrieveDefaultViewFieldIds(workspaceArtifactID, viewArtifactID, artifactTypeID, isProduction);

			return searchManager.RetrieveAllExportableViewFields(workspaceArtifactID, (int)ArtifactType.Document)
				.Where(x => viewFieldIds.Contains(x.AvfId))
				.Select(x => new FieldEntry
				{
					DisplayName = x.DisplayName,
					FieldIdentifier = x.AvfId.ToString(),
					FieldType = FieldType.String,
					IsIdentifier = x.Category == FieldCategory.Identifier,
					IsRequired = false
				}).ToArray();
		}

		public FieldEntry[] GetAllExportableLongTextFields(int workspaceArtifactID, int artifactTypeID)
		{
			ISearchManager searchManager = ServiceManagerProvider.Create<ISearchManager, SearchManagerFactory>(_config, _credentialProvider);

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
	}
}