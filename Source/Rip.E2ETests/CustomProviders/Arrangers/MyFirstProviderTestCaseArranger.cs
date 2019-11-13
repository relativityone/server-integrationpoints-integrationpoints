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
	internal static class MyFirstProviderTestCaseArranger
	{
		private const string _CONTROL_NUMBER_FIELD = "Control Number";
		private const string _EXTRACTED_TEXT_FIELD = "Extracted Text";
        private const string _CUSTOM_PROVIDER_NAME = CustomProvidersConstants.MY_FIRST_PROVIDER_SOURCE_PROVIDER_NAME;

        public static CustomProviderTestCase GetTestCase(int workspaceID)
		{
			string sourceInputFilePath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				"E2ETestData",
				"MyFirstProviderInput.xml");

			string inputFilePathInRelativity = DataTransferDirectoryTestHelper.CopyFileToImportFolder(workspaceID, sourceInputFilePath);
			Dictionary<string, string> nameToTextDictionary = MyFirstProviderInputFileParser.GetNameToTextForInputFilesMapping(sourceInputFilePath);
			var workspaceFieldsToFileFieldsMapping = new Dictionary<string, string>
			{
				[_CONTROL_NUMBER_FIELD] = "Name",
				[_EXTRACTED_TEXT_FIELD] = "Text"
			};

			return new CustomProviderTestCase
			{
                CustomProviderName = _CUSTOM_PROVIDER_NAME,
                InputFilePath = inputFilePathInRelativity,
				TargetRdoArtifactName = "Document",
				WorkspaceFieldsToFileFieldsMapping = workspaceFieldsToFileFieldsMapping,
				IdentifierFieldName = _CONTROL_NUMBER_FIELD,
				MaximumExecutionTime = TimeSpan.FromMinutes(2),
				ExpectedTotalItems = 11,
				ExpectedItemsTransferred = 10,
				ExpectedStatus = JobStatusChoices.JobHistoryCompletedWithErrors,
				ExpectedDocumentsIdentifiersToExtractedTextMapping = nameToTextDictionary
			};
		}
	}
}
