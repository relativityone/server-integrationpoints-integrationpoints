﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
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

		public static int CreateSavedSearch(int workspaceID, string name)
		{
			var keywordSearch = new KeywordSearch
			{
				ArtifactTypeID = (int)ArtifactType.Document,
				Name = name,
				Fields = new List<FieldRef> { new FieldRef("Control Number") }
			};

			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				return proxy.CreateSingleAsync(workspaceID, keywordSearch).GetAwaiter().GetResult();
			}
		}

		public static void UpdateSavedSearchCriteria(int workspaceArtifactID, int searchArtifactID, CriteriaCollection searchCriteria)
		{
			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				KeywordSearch keywordSearch = proxy.ReadSingleAsync(workspaceArtifactID, searchArtifactID).Result;
				keywordSearch.SearchCriteria = searchCriteria;
				proxy.UpdateSingleAsync(workspaceArtifactID, keywordSearch).GetAwaiter().GetResult();
			}
		}

		public static void Delete(int workspaceArtifactID, int savedSearchArtifactID)
		{
			if (savedSearchArtifactID == 0)
			{
				return;
			}
			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				proxy.DeleteSingleAsync(workspaceArtifactID, savedSearchArtifactID).GetAwaiter().GetResult();
			}
		}

		public static int Create(int workspaceArtifactID, KeywordSearch search)
		{
			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				return proxy.CreateSingleAsync(workspaceArtifactID, search).GetAwaiter().GetResult();
			}
		}

		public static async Task<KeywordSearch> ReadAsync(int workspaceArtifactID, int searchArtifactID)
		{
			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				return await proxy.ReadSingleAsync(workspaceArtifactID, searchArtifactID).ConfigureAwait(false);
			}
		}
		
		public static int CreateSearchFolder(int workspaceArtifactID, SearchContainer searchContainer)
		{
			using (var proxy = Helper.CreateAdminProxy<ISearchContainerManager>())
			{
				return proxy.CreateSingleAsync(workspaceArtifactID, searchContainer).GetAwaiter().GetResult();
			}
		}

		public static void ModifySavedSearchByAddingPrefix(IRepositoryFactory repositoryFactory, int workspaceID, int savedSearchID, string documentPrefix, bool excludeExpDocs)
		{
			IFieldQueryRepository sourceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(workspaceID);
			int controlNumberFieldArtifactID = sourceFieldQueryRepository.RetrieveIdentifierField((int)ArtifactType.Document).ArtifactId;

			var fieldRef = new FieldRef(controlNumberFieldArtifactID)
			{
				Name = "Control Number",
				Guids = new List<Guid> { new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438") }
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
			UpdateSavedSearchCriteria(workspaceID, savedSearchID, searchCriteria);
		}
	}
}