﻿using kCura.IntegrationPoints.Data.Extensions;
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
	}
}