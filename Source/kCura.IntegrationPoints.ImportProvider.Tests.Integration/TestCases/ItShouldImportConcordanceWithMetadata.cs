﻿using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;

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
			List<Document> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
			Assert.AreEqual(expectedDocs, workspaceContents.Count);

			for (int i = 0; i < expectedDocs; i++)
			{
				Document docResult = workspaceContents[i];

				int docNum = i + 1;
				Assert.AreEqual(docNum, docResult.DocumentIdentifier);
				Assert.AreEqual($"Row-{docNum}-EmailSubject", docResult.ReadAsString(TestConstants.FieldNames.EMAIL_SUBJECT));
				Assert.AreEqual($"Row-{docNum}-GroupIdentifier", docResult.ReadAsString(TestConstants.FieldNames.GROUP_IDENTIFIER));
			}
		}
	}
}
