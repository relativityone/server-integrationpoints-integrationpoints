namespace kCura.IntegrationPoint.Tests.Core
{
	using Relativity.Client;

	public class SavedSearch : HelperBase
	{
		private const string _createSingleService = "Relativity.Services.Search.ISearchModule/Keyword Search Manager/CreateSingleAsync";

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
			string output = Helper.Rest.PostRequestAsJsonAsync(_createSingleService, SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, false, json);
			return int.Parse(output);
		}
	}
}