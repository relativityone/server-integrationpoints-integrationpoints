using kCura.Relativity.Client;

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
			", workspaceId, (int)ArtifactType.Document, name);
			string output = Rest.PostRequestAsJson(_CREATE_SINGLE_SERVICE, false, json);
			return int.Parse(output);
		}
	}
}