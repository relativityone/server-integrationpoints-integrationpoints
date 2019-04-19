using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.Relativity.Client;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Rip.SystemTests.RelativityServices.TestCases;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace Rip.SystemTests.RelativityServices.Arrangers
{
	public static class DocumentTestCaseArranger
	{
		private const string _CONTROL_NUMBER_COLUMN_NAME = "ControlNumber";
		private const string _FILE_COLUMN_NAME = "File";
		private const string _CONTROL_NUMBER_DISPLAY_COLUMN_NAME = "Control Number";
		private const string _FILENAME_DISPLAY_COLUMN_NAME = "File Name";

		public static async Task FillTestCasesWithDocumentArtifactIDsAsync(
			int workspaceID,
			IEnumerable<DocumentTestCase> documentTestCases, 
			IObjectManager objectManager)
		{
			IList<RelativityObject> documents = await FetchDocumentsAsync(
					workspaceID,
					objectManager, 
					take: documentTestCases.Count()
				)
				.ConfigureAwait(false);

			var files = documents.Select(x => new
			{
				ArtifactID = x.ArtifactID,
				ControlNumber = x.FieldValues
					.Single(fv => fv.Field.Name == _CONTROL_NUMBER_COLUMN_NAME)
					.Value as string
			}).ToArray();

			foreach (var testCase in documentTestCases)
			{
				testCase.ArtifactID = files
					.Single(x => x.ControlNumber == testCase.ControlNumber)
					.ArtifactID;
				foreach (var image in testCase.Images)
				{
					image.DocumentArtifactID = testCase.ArtifactID;
				}
			}
		}

		private static async Task<IList<RelativityObject>> FetchDocumentsAsync(
			int workspaceID,
			IObjectManager objectManager, 
			int take)
		{
			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = (int)ArtifactType.Document
				},
				Fields = new[]
				{
					new FieldRef
					{
						Name = _CONTROL_NUMBER_COLUMN_NAME
					}
				}
			};

			QueryResult queryResult =
				await objectManager
					.QueryAsync(workspaceID, queryRequest, 0, take)
					.ConfigureAwait(false);

			return queryResult.Objects;
		}


		public static DocumentTestCase[] CreateTestCases(DocumentsTestData documentsTestData)
		{
			DataRow[] documentRows = documentsTestData.Documents.First().Documents.Select();
			DataRow[] imageRows = documentsTestData.Images.Select();

			return documentRows
				.Select(documentRow => new {
					ControlNumber = documentRow[_CONTROL_NUMBER_DISPLAY_COLUMN_NAME].ToString(),
					FileName = documentRow[_FILENAME_DISPLAY_COLUMN_NAME].ToString(),
					Row = documentRow
				})
				.Select(documentRow => new DocumentTestCase
				{
					ControlNumber = documentRow.ControlNumber,
					FileName = documentRow.FileName,
					Images = imageRows
						.Where(imageRow => imageRow[_CONTROL_NUMBER_DISPLAY_COLUMN_NAME].ToString() == documentRow.ControlNumber)
						.Select(imageRow => new ImageTestCase
						{
							FileName =  Path.GetFileName(imageRow[_FILE_COLUMN_NAME].ToString()),
						})
						.ToArray()
				}).ToArray();
		}
	}
}
