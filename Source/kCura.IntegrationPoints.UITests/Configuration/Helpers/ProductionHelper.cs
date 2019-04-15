using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class ProductionHelper
	{
		private readonly WorkspaceService _workspaceService;

		private readonly TestContext _testContext;

		public ProductionHelper(TestContext testContext)
		{
			_testContext = testContext;
			_workspaceService = new WorkspaceService(new ImportHelper());
		}

		public void CreateProductionSet(string productionName)
		{
			_workspaceService.CreateProductionSet(_testContext.GetWorkspaceId(), productionName);
		}

		public void CreateAndRunProduction(string productionName)
		{
			int savedSearchId = _workspaceService.CreateSavedSearch(new[] { "Control Number" }, _testContext.GetWorkspaceId(), $"ForProduction_{productionName}");

			string placeHolderFilePath = Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, @"TestData\DefaultPlaceholder.tif");

			_workspaceService.CreateAndRunProduction(_testContext.GetWorkspaceId(), savedSearchId, productionName, placeHolderFilePath);
		}

		public void CreateAndRunProduction(string savedSearchName, string productionName)
		{
			int savedSearchId = RetrieveSavedSearchId(savedSearchName);
			_workspaceService.CreateAndRunProduction(_testContext.GetWorkspaceId(), savedSearchId, productionName);
		}

		private int RetrieveSavedSearchId(string savedSearchName)
		{
			var savedSearchRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Search },
				Condition = $"'Name' == '{savedSearchName}'",
				Fields = new FieldRef[0]
			};
			RelativityObject savedSearch = _testContext.ObjectManager.Query(savedSearchRequest).First();
			return savedSearch.ArtifactID;
		}
	}
}
