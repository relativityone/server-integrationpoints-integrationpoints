#if INCLUDE_HARD_CODED_ARTIFACTID_TESTS

// "REL-841500: Resolve RIP functional tests that depend on hard-coded ArtifactID values"

using System.Collections.Generic;
using System.Linq;

using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base;

using NUnit.Framework;

using Relativity.Services.Objects.DataContracts;

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
			List<Document> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
			Assert.AreEqual(expectedDocs, workspaceContents.Count);

			for (int i = 0; i < expectedDocs; i++)
			{
				Document docResult = workspaceContents[i];

				int docNum = i + 1;
				Assert.AreEqual(docNum.ToString(), docResult.ControlNumber);

				Choice custodianChoice = (Choice) docResult[TestConstants.FieldNames.CUSTODIAN];
				Assert.AreEqual(CustodianSingleChoices[i], custodianChoice.Name);

				List<Choice> issueValues = (List<Choice>)docResult[TestConstants.FieldNames.ISSUE_DESIGNATION];
				Assert.AreEqual(3, issueValues.Count);
				foreach (Choice value in issueValues)
				{
					Assert.IsNotEmpty(IssueMultiChoices[i].Where(x => x == value.Name));
				}
			}
		}
	}
}
#endif