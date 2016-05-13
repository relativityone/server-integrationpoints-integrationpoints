using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class SavedSearch : HelperBase
	{
		private const string _CREATE_SINGLE_SERVICE = "Relativity.Services.Search.ISearchModule/Keyword Search Manager/CreateSingleAsync";

		public SavedSearch(Helper helper) : base(helper)
		{
		}

		public int CreateSavedSearch(int workspaceId, string name)
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
			string output = Helper.Rest.PostRequestAsJsonAsync(_CREATE_SINGLE_SERVICE, SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, false, json);
			return int.Parse(output);
		}
	}
}