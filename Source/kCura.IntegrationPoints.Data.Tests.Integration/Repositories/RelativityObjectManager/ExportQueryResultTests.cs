using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories.RelativityObjectManager
{
	[TestFixture]
	public class ExportQueryResultTests
	{
		private int _workspaceId;
		private ImportHelper _importHelper;
		private TestHelper _helper;
		private IRelativityObjectManager _relativityObjectManager;
		private int _allDocumentsSavedSearchId;
		private const string _WORKSPACE_NAME = "RIPExportQueryResultTests";


		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			string workspaceName = GetWorkspaceRandomizedName();
			_workspaceId = Workspace.CreateWorkspace(workspaceName);
			_importHelper = new ImportHelper();
			_helper = new TestHelper();
			_relativityObjectManager = CreateObjectManager();
			_allDocumentsSavedSearchId = await GetSavedSearchInstance().ConfigureAwait(false);
		}

		[Test]
		public async Task ExportQueryResult_ShouldDeleteExport_WhenDisposed()
		{
			// Arrange
			var queryBuilder = new DocumentQueryBuilder();
			QueryRequest query = queryBuilder.AddSavedSearchCondition(_allDocumentsSavedSearchId).NoFields().Build();

			Guid runId = Guid.Empty;

			// Act
			using (var exportQueryResult = await _relativityObjectManager.QueryWithExportAsync(query, 0).ConfigureAwait(false))
			{
				runId = exportQueryResult.RunId;
				IEnumerable<RelativityObjectSlim> results = await exportQueryResult.GetAllResultsAsync().ConfigureAwait(false);
			}

			Func<Task> action = () => _relativityObjectManager.RetrieveResultsBlockFromExportAsync(runId, 10, 0);

			// Assert
			action.ShouldThrow<Exception>();
		}

		public async Task<int> GetSavedSearchInstance()
		{
			const string name = "All Documents";
			using (IKeywordSearchManager keywordSearchManager = _helper.CreateProxy<IKeywordSearchManager>())
			{
				Query request = new Query
				{
					Condition = $"(('Name' == '{name}'))"
				};
				KeywordSearchQueryResultSet result =
					await keywordSearchManager.QueryAsync(_workspaceId, request).ConfigureAwait(false);
				if (result.TotalCount == 0)
				{
					throw new InvalidOperationException(
						$"Cannot find saved search '{name}' in workspace {_workspaceId}");
				}

				return result.Results.First().Artifact.ArtifactID;
			}
		}

		private IRelativityObjectManager CreateObjectManager()
		{
			var factory = new RelativityObjectManagerFactory(_helper);
			return factory.CreateRelativityObjectManager(_workspaceId);
		}

		private IEnumerable<int> GetArtifactIds(IEnumerable<RelativityObjectSlim> relativityObjects) =>
			relativityObjects.Select(GetArtifactId);

		private int GetArtifactId(RelativityObjectSlim relativityObject) => relativityObject.ArtifactID;

		private string GetWorkspaceRandomizedName() =>
			$"{_WORKSPACE_NAME}{System.DateTime.UtcNow.ToString(@"yyyy_M_d_hh_mm_ss")}";
	}
}
