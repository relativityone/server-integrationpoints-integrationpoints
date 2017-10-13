﻿using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
	public class ItShouldImportConcordanceWithMetadata : LoadFileTest
	{
		public override SettingsObjects Prepare(int workspaceId)
		{
			SettingsObjects objects = base.Prepare(workspaceId,
				TestConstants.Resources.CSV_WITH_METADATA,
				TestConstants.LoadFiles.CONCORDANCE_WITH_METADATA);

			objects.ImportProviderSettings.AsciiColumn = 20;
			objects.ImportProviderSettings.AsciiQuote = 254;
			return objects;
		}

		public override void Verify(int workspaceId)
		{
			int expectedDocs = 3;
			List<Result<Document>> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
			Assert.AreEqual(expectedDocs, workspaceContents.Count);

			for (int i = 0; i < expectedDocs; i++)
			{
				Result<Document> docResult = workspaceContents[i];
				FieldValue controlNumber = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.CONTROL_NUMBER);
				FieldValue emailSubject = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.EMAIL_SUBJECT);
				FieldValue groupIdentifier = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.GROUP_IDENTIFIER);

				int docNum = i + 1;
				Assert.AreEqual(docNum.ToString(), controlNumber.ValueAsLongText);
				Assert.AreEqual($"Row-{docNum}-EmailSubject", emailSubject.ValueAsLongText);
				Assert.AreEqual($"Row-{docNum}-GroupIdentifier", groupIdentifier.ValueAsLongText);
			}
		}
	}
}
