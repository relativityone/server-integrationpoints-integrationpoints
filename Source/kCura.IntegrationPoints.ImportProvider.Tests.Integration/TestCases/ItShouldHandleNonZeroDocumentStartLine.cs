﻿using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.Relativity.Client.DTOs;

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
			List<Result<Document>> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
			Assert.AreEqual(expectedDocs, workspaceContents.Count);

			for (int i = 0; i < expectedDocs; i++)
			{
				Result<Document> docResult = workspaceContents[i];
				FieldValue controlNumber = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.CONTROL_NUMBER);
				FieldValue emailSubject = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.EMAIL_SUBJECT);
				FieldValue groupIdentifier = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.GROUP_IDENTIFIER);

				int docNum = i + 2;
				Assert.AreEqual(docNum.ToString(), controlNumber.ValueAsLongText);
				Assert.AreEqual($"Row-{docNum}-EmailSubject", emailSubject.ValueAsLongText);
				Assert.AreEqual($"Row-{docNum}-GroupIdentifier", groupIdentifier.ValueAsLongText);
			}
		}
	}
}
