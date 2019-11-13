using System;
using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Rip.E2ETests.Constants;
using Rip.E2ETests.CustomProviders.Helpers;
using Rip.E2ETests.CustomProviders.TestCases;

namespace Rip.E2ETests.CustomProviders.Arrangers
{
	internal static class JsonLoaderTestCaseArranger
	{
		private const string _NAME = "Name";
		private const string _SAMPLE_TEXT_FIELD = "Sample Text Field";
		private const string _CUSTOM_PROVIDER_NAME = CustomProvidersConstants.JSON_LOADER_SOURCE_PROVIDER_NAME;

		public static CustomProviderTestCase GetTestCase(int workspaceID)
		{
			string sourceFieldsFilePath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				"E2ETestData",
				"JsonLoaderFields.json");
			string sourceDataFilePath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				"E2ETestData",
				"JsonLoaderData.json");

			string fieldFilePathInRelativity =
				DataTransferDirectoryTestHelper.CopyFileToImportFolder(workspaceID, sourceFieldsFilePath);
			string dataFilePathInRelativity =
				DataTransferDirectoryTestHelper.CopyFileToImportFolder(workspaceID, sourceDataFilePath);
			Dictionary<string, string> nameToTextDictionary =
				JsonLoaderInputFileParser.GetNameToSampleTextForInputFilesMapping(sourceDataFilePath);
			var workspaceFieldsToFileFieldsMapping = new Dictionary<string, string>
			{
				[_NAME] = "ID0",
				[_SAMPLE_TEXT_FIELD] = "ID1"
			};
			var jsonLoaderSourceFieldFieldIdentifierToDisplayNameMapping = new Dictionary<string, string>
			{
				["ID0"] = "ID",
				["ID1"] = "Filename"
			};

			return new CustomProviderTestCase
			{
				CustomProviderName = _CUSTOM_PROVIDER_NAME,
				FieldFilePath = fieldFilePathInRelativity,
				DataFilePath = dataFilePathInRelativity,
				TargetRdoArtifactName = "Sample JSON Object",
				WorkspaceFieldsToFileFieldsMapping = workspaceFieldsToFileFieldsMapping,
				SourceFieldFieldIdentifierToDisplayNameMapping =
					jsonLoaderSourceFieldFieldIdentifierToDisplayNameMapping,
				IdentifierFieldName = _NAME,
				MaximumExecutionTime = TimeSpan.FromMinutes(2),
				ExpectedTotalItems = 11,
				ExpectedItemsTransferred = 11,
				ExpectedStatus = JobStatusChoices.JobHistoryCompleted,
				ExpectedDocumentsIdentifiersToExtractedTextMapping = nameToTextDictionary
			};
		}
	}
}