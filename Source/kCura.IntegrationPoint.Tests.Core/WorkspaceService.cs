﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class WorkspaceService
	{
		#region Constructors

		public WorkspaceService(ImportHelper importHelper)
		{
			_importHelper = importHelper;
		}

		#endregion //Constructors

		#region Fields

		private readonly ImportHelper _importHelper;

		private const string _TEMPLATE_WORKSPACE_NAME = "Relativity Starter Template";
		private const string _LEGACY_TEMPLATE_WORKSPACE_NAME = "kCura Starter Template";
		private const string _SAVED_SEARCH_FOLDER = "Testing Folder";
		private const string _SAVED_SEARCH_NAME = "Testing Saved Search";

		#endregion //Fields

		#region Properties

		public static string StarterTemplateName => SharedVariables.UseLegacyTemplateName()
			? _LEGACY_TEMPLATE_WORKSPACE_NAME
			: _TEMPLATE_WORKSPACE_NAME;

		#endregion

		#region Methods



		public int CreateWorkspace(string name, string template = null)
		{
			string templateName = template ?? StarterTemplateName;
			return Workspace.CreateWorkspace(name, templateName);
		}

		public void ImportData(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			_importHelper.ImportData(workspaceArtifactId, documentsTestData);
		}

		public void DeleteWorkspace(int artifactId)
		{
			using (var rsApiClient = Rsapi.CreateRsapiClient(ExecutionIdentity.System))
			{
				rsApiClient.Repositories.Workspace.DeleteSingle(artifactId);
			}
		}

		public int CreateSavedSearch(FieldEntry[] defaultFields, FieldEntry[] additionalFields, int workspaceId, string savedSearchName)
		{
			var fields = defaultFields
				.Select(x => new FieldRef(x.DisplayName))
				.Concat(additionalFields.Select(x => new FieldRef(x.DisplayName)))
				.ToList();

			return CreateSavedSearch(fields, workspaceId, savedSearchName);
		}

		public int CreateSavedSearch(IEnumerable<string> fields, int workspaceId, string savedSearchName)
		{
			return CreateSavedSearch(fields.Select(displayName => new FieldRef(displayName)).ToList(), workspaceId,
				savedSearchName);
		}

		public int CreateSavedSearch(List<FieldRef> fields, int workspaceId, string savedSearchName)
		{
			var folder = new SearchContainer
			{
				Name = _SAVED_SEARCH_FOLDER
			};
			var folderArtifactId = Core.SavedSearch.CreateSearchFolder(workspaceId, folder);

			var search = new KeywordSearch
			{
				Name = savedSearchName,
				ArtifactTypeID = (int) ArtifactType.Document,
				SearchContainer = new SearchContainerRef(folderArtifactId),
				Fields = fields
			};
			return Core.SavedSearch.Create(workspaceId, search);
		}

		public int CreateSavedSearch(FieldEntry[] defaultFields, FieldEntry[] additionalFields, int workspaceId)
		{
			return CreateSavedSearch(defaultFields, additionalFields, workspaceId, _SAVED_SEARCH_NAME);
		}

		public int CreateProductionSet(int workspaceArtifactId, string productionSetName)
		{
			return Production.Create(workspaceArtifactId, productionSetName);
		}

		public int CreateAndRunProduction(int workspaceArtifactId, int savedSearchId, string productionName)
		{
			var placeHolderFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\DefaultPlaceholder.tif");

			return CreateAndRunProduction(workspaceArtifactId, savedSearchId, productionName, placeHolderFilePath);
		}

		public int CreateAndRunProduction(int workspaceArtifactId, int savedSearchId, string productionName, string placeHolderFilePath)
		{
			var placeHolderFileData = FileToBase64Converter.Convert(placeHolderFilePath);

			var productionId = CreateProductionSet(workspaceArtifactId, productionName);

			var placeholderId = Placeholder.Create(workspaceArtifactId, placeHolderFileData);
			ProductionDataSource.CreateDataSourceWithPlaceholder(workspaceArtifactId, productionId, savedSearchId,
				"WhenNoImageExists", placeholderId);

			Production.StageAndWaitForCompletion(workspaceArtifactId, productionId);
			Production.RunAndWaitForCompletion(workspaceArtifactId, productionId);

			return productionId;
		}

		public int GetView(int workspaceId, string viewName)
		{
			return View.QueryView(workspaceId, viewName);
		}

		#endregion Methods
	}
}