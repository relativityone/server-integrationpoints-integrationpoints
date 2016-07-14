using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportFieldsService : IExportFieldsService
	{
		private readonly ICredentialProvider _credentialProvider;

		private ISearchManager CreateSearchManager()
		{
			var cookieContainer = new CookieContainer();
			var credentials = _credentialProvider.Authenticate(cookieContainer);

			var searchManager = (new SearchManagerFactory()).Create(credentials, cookieContainer);

			return searchManager;
		}

		public ExportFieldsService(ICredentialProvider credentialProvider)
		{
			_credentialProvider = credentialProvider;
		}

		public FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID)
		{
			ISearchManager searchManager = CreateSearchManager();

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
			ISearchManager searchManager = CreateSearchManager();

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
	}
}