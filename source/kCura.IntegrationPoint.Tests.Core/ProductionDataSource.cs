namespace kCura.IntegrationPoint.Tests.Core
{
	public static class ProductionDataSource
	{
		private const string _PRODUCTION_DATA_SOURCE_SERVICE_BASE =
			"api/Relativity.Productions.Services.IProductionModule/Production Data Source Manager/";

		private const string _CREATE_SINGLE_SERVICE =
			_PRODUCTION_DATA_SOURCE_SERVICE_BASE + "CreateSingleAsync";

		public static int Create(int workspaceId, int productionId, int savedSearchId, string useImagePlaceholder)
		{
			var json =
				$@"
					{{
					  ""workspaceArtifactID"": {workspaceId},
					  ""productionID"": {productionId},
					  ""dataSource"": {{						
						""ProductionType"": ""ImagesOnly"",
						""SavedSearch"": {{
						  ""ArtifactID"": {savedSearchId}
						}},
						""UseImagePlaceholder"": ""{useImagePlaceholder}"",
						""Placeholder"": {{
							""ArtifactID"": 1039165,
							""Name"": ""Default""
						  }},
						""MarkupSet"": {{
						  ""ArtifactID"": 1034197
						}},
						""BurnRedactions"": true,
						""Name"": ""TestDataSource""
					  }}
					}}";

			string output = Rest.PostRequestAsJson(_CREATE_SINGLE_SERVICE, false, json);
			return int.Parse(output);
		}
	}
}