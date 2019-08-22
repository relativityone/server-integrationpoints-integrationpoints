using System;
using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NUnit.Framework;
using Rip.E2ETests.CustomProviders.Helpers;
using Rip.E2ETests.CustomProviders.TestCases;

namespace Rip.E2ETests.CustomProviders.Arrangers
{
	internal static class MyFirstProviderTestCaseArranger
	{
		private const string _CONTROL_NUMBER_FIELD = "Control Number";
		private const string _EXTRACTED_TEXT_FIELD = "Extracted Text";

		public static MyFirstProviderTestCase GetTestCase(int workspaceID)
		{
			string sourceInputFilePath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				"E2ETestData",
				"MyFirstProviderInput.xml");

			string inputFilePathInRelativity = DataTransferDirectoryRepository.CopyFileToImportFolder(workspaceID, sourceInputFilePath);
			Dictionary<string, string> nameToTextDictionary = MyFirstProviderInputFileParser.GetNameToTextForInputFilesMapping(sourceInputFilePath);
			var workspaceFieldsToFileFieldsMapping = new Dictionary<string, string>
			{
				[_CONTROL_NUMBER_FIELD] = "Name",
				[_EXTRACTED_TEXT_FIELD] = "Text"
			};

			return new MyFirstProviderTestCase
			{
				InputFilePath = inputFilePathInRelativity,
				TargetRdoArtifactID = (int)ArtifactType.Document,
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
