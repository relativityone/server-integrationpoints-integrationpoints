using Relativity.Services.Production;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class Placeholder
	{
		private const string _PRODUCTION_PLACEHOLDER_SERVICE_BASE =
			@"/Relativity.REST/api/Relativity.Productions.Services.IProductionModule/Production%20Placeholder%20Manager/";

		private const string _CREATE_SINGLE_SERVICE =
			_PRODUCTION_PLACEHOLDER_SERVICE_BASE + "CreateSingleAsync";

		public static int Create(int workspaceId, string fileData)
		{
			var json =
				$@"
					{{
					  ""workspaceArtifactID"": {workspaceId},
					  ""placeholder"":
						{{
							""PlaceholderType"": ""Image"", 
							""FileData"": ""{fileData}"",
							""Filename"": ""DefaultPlaceholder.tif"",
							""Name"": ""CustomPlaceholder""
						}}
					}}";

			//var json =
			//	$@"
			//		{{
			//		  ""workspaceArtifactID"": {workspaceId},
			//		  ""placeholder"":
			//			{{
			//				""PlaceholderType"": ""Custom"",
			//				""CustomText"": ""<div style=\""text-align:center;\""><span style=\""font-weight:bold;font-size:24px;font-family:Arial;\"">No image available for this record.</span></div>"",
			//				""Name"": ""CustomPlaceholder""
			//			}}
			//		}}";

			string output = Rest.PostRequestAsJson(_CREATE_SINGLE_SERVICE, false, json);
			return int.Parse(output);
		}
	}
}