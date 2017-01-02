using System;
using System.Threading;
using Newtonsoft.Json;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Production
	{
		private const int _MAX_RETRIES_COUNT = 100;

		private const string _PRODUCTION_SERVICE_URL_BASE =
			"api/Relativity.Productions.Services.IProductionModule/Production%20Manager/";

		private const string _CREATE_PRODUCTION_SERVICE =
			_PRODUCTION_SERVICE_URL_BASE + "CreateSingleAsync";

		private const string _STAGE_PRODUCTION_SERVICE =
			_PRODUCTION_SERVICE_URL_BASE + "StageProductionAsync";

		private const string _RUN_PRODUCTION_SERVICE =
			_PRODUCTION_SERVICE_URL_BASE + "RunProductionAsync";

		private const string _READ_PRODUCTION_SERVICE =
			_PRODUCTION_SERVICE_URL_BASE + "/ReadSingleAsync";

		public static int Create(int workspaceId, string productionName)
		{
			var json =
				$@"
				{{
					""workspaceArtifactID"": {workspaceId},
					""Production"": {{
						""Details"": {{
							""BrandingFontSize"": 10,
							""ScaleBrandingFont"": false
						}},
						""Numbering"": {{
								""NumberingType"": ""DocumentField"",
								""NumberingField"":{{  
										 ""ArtifactID"":1003667,
										 ""ViewFieldID"":0,
										 ""Name"":""Control Number""
								}},
								""AttachmentRelationalField"": {{
										""ArtifactID"": 0,
										""ViewFieldID"": 0,
										""Name"": """"
								}},							  
								""BatesPrefix"": """",
								""BatesSuffix"": """",								
								""BatesStartNumber"": 1,
								""IncludePageNumbers"":false,
								""DocumentNumberPageNumberSeparator"":"""",
								""NumberOfDigitsForPageNumbering"":0,
								""NumberOfDigitsForDocumentNumbering"": 7,
								""StartNumberingOnSecondPage"":false
						}},								
						""ShouldCopyInstanceOnWorkspaceCreate"": false,
						""Name"": ""{productionName}""
					}}
				}}";

			var output = Rest.PostRequestAsJson(_CREATE_PRODUCTION_SERVICE, false, json);
			return int.Parse(output);
		}

		public static void StageAndWaitForCompletion(int workspaceId, int productionId)
		{
			var json =
				$@"
						{{
						  ""workspaceArtifactID"": {workspaceId},
						  ""productionArtifactID"": {productionId}
						}}";

			Rest.PostRequestAsJson(_STAGE_PRODUCTION_SERVICE, false, json);

			WaitForProductionStatus(workspaceId, productionId, "Staged");
		}

		public static void RunAndWaitForCompletion(int workspaceId, int productionId, bool suppressWarnings = true, bool overrideConflicts = false)
		{
			var json =
				$@"
				{{
				  ""workspaceArtifactID"": {workspaceId},
				  ""productionArtifactID"": {productionId},
				  ""suppressWarnings"": true,
				  ""overrideConflicts"": false
				}}";

			Rest.PostRequestAsJson(_RUN_PRODUCTION_SERVICE, false, json);

			WaitForProductionStatus(workspaceId, productionId, "Produced");
		}

		private static void WaitForProductionStatus(int workspaceId, int productionId, string expectedStatus)
		{
			for (int i = 0; i < _MAX_RETRIES_COUNT; i++)
			{
				var status = GetProductionStatus(workspaceId, productionId);

				if (status == expectedStatus)
				{
					break;
				}

				if (status.Contains("Error"))
				{
					throw new Exception("ProductionOperation finished with errors");
				}

				Thread.Sleep(1000);
			}
		}

		private static string GetProductionStatus(int workspaceId, int productionId)
		{
			var json =
				$@"
				{{
					""workspaceArtifactID"": {workspaceId},
					""productionArtifactID"": {productionId}
				}}";

			var output = Rest.PostRequestAsJson(_READ_PRODUCTION_SERVICE, false, json);
			var status = ExtractStatusFromProductionDtoJson(output);
			return status;
		}

		private static string ExtractStatusFromProductionDtoJson(string productionDtoJson)
		{
			var productionObject = JsonConvert.DeserializeObject<dynamic>(productionDtoJson);
			var productionMetadata = productionObject["ProductionMetadata"];
			var status = productionMetadata["Status"];
			return status.ToString();
		}
	}
}