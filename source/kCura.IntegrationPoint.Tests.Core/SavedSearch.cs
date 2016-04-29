using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core
{
	using Relativity.Client;

	public class SavedSearch : HelperBase
	{
		private const string _createSingleService = "Relativity.Services.Search.ISearchModule/Keyword Search Manager/CreateSingleAsync";
		public SavedSearch(Helper helper) : base(helper)
		{
		}

		public int CreateSavedSearch(string restServer, string userName, string userPassword, int workspaceId, string name)
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
			string output = Helper.Rest.PostRequestAsJsonAsync(restServer, _createSingleService, userName, userPassword, false, json);
			return int.Parse(output);
		}
	}
}