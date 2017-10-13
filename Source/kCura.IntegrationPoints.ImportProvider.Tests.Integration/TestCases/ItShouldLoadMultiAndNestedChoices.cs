using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base;
using kCura.Relativity.Client.DTOs;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases
{
	public class ItShouldLoadMultiAndNestedChoices : LoadFileTest
	{
		public override SettingsObjects Prepare(int workspaceId)
		{
			return base.Prepare(workspaceId,
				TestConstants.Resources.CSV_WITH_CHOICES,
				TestConstants.LoadFiles.CSV_WITH_CHOICES);
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
				FieldValue custodian = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.CUSTODIAN);
				FieldValue issueDesignation = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.ISSUE_DESIGNATION);

				MultiChoiceFieldValueList issueValues = issueDesignation.ValueAsMultipleChoice;
				Choice custodianValue = custodian.ValueAsSingleChoice;

				int docNum = i + 1;
				Assert.AreEqual(docNum.ToString(), controlNumber.ValueAsLongText);
				Assert.AreEqual(CustodianSingleChoices[i], custodianValue.Name);
				Assert.AreEqual(3, issueValues.Count);
				foreach (Choice value in issueValues)
				{
					string[] expectedRow = IssueMultiChoices[i];
					Assert.IsNotEmpty(IssueMultiChoices[i].Where(x => x == value.Name));
				}
			}
		}
	}
}
