using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base;

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

				List<string> issueValues = (List<string>) docResult[TestConstants.FieldNames.ISSUE_DESIGNATION];

				int docNum = i + 1;
				Assert.AreEqual(docNum.ToString(), docResult.ControlNumber);
				Assert.AreEqual(CustodianSingleChoices[i], docResult[TestConstants.FieldNames.CUSTODIAN]);
				Assert.AreEqual(3, issueValues.Count);
				foreach (string value in issueValues)
				{
					Assert.IsNotEmpty(IssueMultiChoices[i].Where(x => x == value));
				}
			}
		}
	}
}
