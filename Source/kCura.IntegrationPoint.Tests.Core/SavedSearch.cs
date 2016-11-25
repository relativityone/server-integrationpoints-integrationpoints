using System;
using System.Collections.Generic;
using Castle.Windsor;
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
		private const string _CREATE_SINGLE_SERVICE = "api/Relativity.Services.Search.ISearchModule/Keyword Search Manager/CreateSingleAsync";

		public static int CreateSavedSearch(int workspaceId, string name)
		{
			string json = string.Format(@"
				{{
					workspaceArtifactID: {0},
					searchDTO: {{
						ArtifactTypeID: {1},
						Name: ""{2}"",
						Fields: [
							{{
								Name: ""Control Number""
							}}
						]
					}}
				}}
			", workspaceId, (int)Relativity.Client.ArtifactType.Document, name);
			string output = Rest.PostRequestAsJson(_CREATE_SINGLE_SERVICE, false, json);
			return int.Parse(output);
		}

		public static void UpdateSavedSearchCriteria(int workspaceArtifactId, int searchArtifactId, CriteriaCollection searchCriteria)
		{
			using (IKeywordSearchManager proxy = Kepler.CreateProxy<IKeywordSearchManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				KeywordSearch keywordSearch = proxy.ReadSingleAsync(workspaceArtifactId, searchArtifactId).Result;
				keywordSearch.SearchCriteria = searchCriteria;
				proxy.UpdateSingleAsync(workspaceArtifactId, keywordSearch).GetAwaiter().GetResult();
			}
		}

		public static void Delete(int workspaceArtifactId, int savedSearchArtifactId)
		{
			if (savedSearchArtifactId == 0) { return; }
			using (IKeywordSearchManager proxy = Kepler.CreateProxy<IKeywordSearchManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				proxy.DeleteSingleAsync(workspaceArtifactId, savedSearchArtifactId).GetAwaiter().GetResult();
			}
		}

		public static int Create(int workspaceArtifactId, KeywordSearch search)
		{
			using (IKeywordSearchManager proxy = Kepler.CreateProxy<IKeywordSearchManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				return proxy.CreateSingleAsync(workspaceArtifactId, search).GetResultsWithoutContextSync();
			}
		}

		public static int CreateSearchFolder(int workspaceArtifactId, SearchContainer searchContainer)
		{
			using (ISearchContainerManager proxy = Kepler.CreateProxy<ISearchContainerManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				return proxy.CreateSingleAsync(workspaceArtifactId, searchContainer).GetResultsWithoutContextSync();
			}
		}

		public static void ModifySavedSearchByAddingPrefix(IWindsorContainer container, int workspaceId, int savedSearchId, string documentPrefix, bool excludeExpDocs)
		{
			IFieldRepository sourceFieldRepository = container.Resolve<IRepositoryFactory>().GetFieldRepository(workspaceId);
			int controlNumberFieldArtifactId = sourceFieldRepository.RetrieveTheIdentifierField((int)ArtifactType.Document).ArtifactId;

			FieldRef fieldRef = new FieldRef(controlNumberFieldArtifactId)
			{
				Name = "Control Number",
				Guids = new List<Guid>() { new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438") }
			};

			CriteriaCollection searchCriteria = new CriteriaCollection();

			if (excludeExpDocs)
			{
				Criteria criteria = new Criteria()
				{
					BooleanOperator = BooleanOperatorEnum.None,
					Condition = new CriteriaCondition(fieldRef, CriteriaConditionEnum.IsLike, documentPrefix),
				};

				searchCriteria.Conditions.Add(criteria);
			}
			SavedSearch.UpdateSavedSearchCriteria(workspaceId, savedSearchId, searchCriteria);
		}
	}
}