using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class SavedSearch
	{
		private static ITestHelper Helper => new TestHelper();

		public static int CreateSavedSearch(int workspaceId, string name)
		{
			var keywordSearch = new KeywordSearch
			{
				ArtifactTypeID = (int)ArtifactType.Document,
				Name = name,
				Fields = new List<FieldRef> { new FieldRef("Control Number") }
			};

			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				return proxy.CreateSingleAsync(workspaceId, keywordSearch).GetAwaiter().GetResult();
			}
		}

		public static void UpdateSavedSearchCriteria(int workspaceArtifactId, int searchArtifactId, CriteriaCollection searchCriteria)
		{
			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				KeywordSearch keywordSearch = proxy.ReadSingleAsync(workspaceArtifactId, searchArtifactId).Result;
				keywordSearch.SearchCriteria = searchCriteria;
				proxy.UpdateSingleAsync(workspaceArtifactId, keywordSearch).GetAwaiter().GetResult();
			}
		}

		public static void Delete(int workspaceArtifactId, int savedSearchArtifactId)
		{
			if (savedSearchArtifactId == 0)
			{
				return;
			}
			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				proxy.DeleteSingleAsync(workspaceArtifactId, savedSearchArtifactId).GetAwaiter().GetResult();
			}
		}

		public static int Create(int workspaceArtifactId, KeywordSearch search)
		{
			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				return proxy.CreateSingleAsync(workspaceArtifactId, search).GetResultsWithoutContextSync();
			}
		}

		public static int CreateSearchFolder(int workspaceArtifactId, SearchContainer searchContainer)
		{
			using (var proxy = Helper.CreateAdminProxy<ISearchContainerManager>())
			{
				return proxy.CreateSingleAsync(workspaceArtifactId, searchContainer).GetResultsWithoutContextSync();
			}
		}

		public static void ModifySavedSearchByAddingPrefix(IRepositoryFactory repositoryFactory, int workspaceId, int savedSearchId, string documentPrefix, bool excludeExpDocs)
		{
			IFieldQueryRepository sourceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(workspaceId);
			int controlNumberFieldArtifactId = sourceFieldQueryRepository.RetrieveTheIdentifierField((int) ArtifactType.Document).ArtifactId;

			var fieldRef = new FieldRef(controlNumberFieldArtifactId)
			{
				Name = "Control Number",
				Guids = new List<Guid> {new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438")}
			};

			var searchCriteria = new CriteriaCollection();

			if (excludeExpDocs)
			{
				var criteria = new Criteria
				{
					BooleanOperator = BooleanOperatorEnum.None,
					Condition = new CriteriaCondition(fieldRef, CriteriaConditionEnum.IsLike, documentPrefix)
				};

				searchCriteria.Conditions.Add(criteria);
			}
			UpdateSavedSearchCriteria(workspaceId, savedSearchId, searchCriteria);
		}
	}
}