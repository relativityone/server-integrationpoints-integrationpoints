﻿using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
	public class ItShouldHandleNonZeroDocumentStartLine : LoadFileTest
	{
		public override SettingsObjects Prepare(int workspaceId)
		{
			SettingsObjects settings = base.Prepare(workspaceId,
				TestConstants.Resources.CSV_WITH_METADATA,
				TestConstants.LoadFiles.CSV_WITH_METADATA);

			settings.ImportProviderSettings.LineNumber = "1";
			return settings;
		}

		public override void Verify(int workspaceId)
		{
			int expectedDocs = 2;
			List<Document> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
			Assert.AreEqual(expectedDocs, workspaceContents.Count);

			for (int i = 0; i < expectedDocs; i++)
			{
				Document docResult = workspaceContents[i];

				int docNum = i + 2;
				Assert.AreEqual(docNum.ToString(), docResult.ControlNumber);
				Assert.AreEqual($"Row-{docNum}-EmailSubject", docResult.ReadAsString(TestConstants.FieldNames.EMAIL_SUBJECT));
				Assert.AreEqual($"Row-{docNum}-GroupIdentifier", docResult.ReadAsString(TestConstants.FieldNames.GROUP_IDENTIFIER));
			}
		}
	}
}
