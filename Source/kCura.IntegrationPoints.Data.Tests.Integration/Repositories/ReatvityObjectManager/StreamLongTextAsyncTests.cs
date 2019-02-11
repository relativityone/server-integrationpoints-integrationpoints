using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using ArtifactType = Relativity.ArtifactType;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories.RelativityObjectManager
{
	[TestFixture]
	public class StreamLongTextAsyncTests
	{
		private int _workspaceId;
		private IHelper _helper;
		private IRelativityObjectManager _relativityObjectManager;
		private ImportHelper _importHelper;
		private WorkspaceService _workspaceService;

		private const string _WORKSPACE_NAME = "RIPStreamLongTextTests";
		private const string _EXTRACTED_TEXT_FIELD_NAME = "Extracted Text";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string workspaceName = GetWorkspaceRandomizedName();
			_workspaceId = Workspace.CreateWorkspace(workspaceName, SourceProviderTemplate.WorkspaceTemplates.NEW_CASE_TEMPLATE);
			_importHelper = new ImportHelper();
			_workspaceService = new WorkspaceService(_importHelper);
			_helper = new TestHelper();
			_relativityObjectManager = CreateObjectManager();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Workspace.DeleteWorkspace(_workspaceId);
		}

		[Test]
		public async Task ItShouldFetchDocumentWith15MBExtractedText()
		{
			int bytes = GetBytesFromMB(15);
			await ExecuteTest(bytes);
		}

		[Test]
		// TODO change to StressTest attribute
		public async Task ItShouldFetchDocumentWith1500MBExtractedText()
		{
			int bytes = GetBytesFromMB(1500);
			await ExecuteTest(bytes);
		}

		private int GetBytesFromMB(int sizeInMB) => sizeInMB * 1024 * 1024;

		private async Task ExecuteTest(int textSizeInBytes)
		{
			// Arrange
			string controlNumber = GetRandomControlNumber();
			string extractedText = DocumentTestDataBuilder.GenerateRandomExtractedText(textSizeInBytes);

			_workspaceService.ImportExtractedTextSimple(_workspaceId, controlNumber, extractedText);
			int documentArtifactID = GetDocumentArtifactID(controlNumber);

			// Act
			System.IO.Stream actualExtractedTextStream = 
				await _relativityObjectManager.StreamLongTextAsync(
					documentArtifactID, 
					new FieldRef { Name = _EXTRACTED_TEXT_FIELD_NAME});
			var actualExtractedTextStreamReader = new StreamReader(actualExtractedTextStream, Encoding.UTF8);
			string actualExtractedTextString = actualExtractedTextStreamReader.ReadToEnd();

			// Assert
			Assert.AreEqual(
				extractedText.Length,
				actualExtractedTextString.Length,
				"Extracted Text returned by ObjectManager should be the same length as original text!");

			IEnumerable<int> charsIndexes = GetExponentialIndexes(extractedText.Length);
			ValidateSpecificCharacters(charsIndexes, extractedText, actualExtractedTextString);
		}

		private string GetRandomControlNumber()
			=> Guid.NewGuid().ToString().Replace("-", "");

		private int[] GetExponentialIndexes(int size)
		{
			int @base = 2;
			int lastExponent = (int)Math.Floor(Math.Log(size, @base));
			return new int[] {0}
				.Concat(Enumerable.Range(0, lastExponent))
				.Select(x => (int)Math.Pow(@base, x))
				.Concat(new int[] {size - 1})
				.ToArray();
		}

		private void ValidateSpecificCharacters(IEnumerable<int> positions, string expectedString, string actualString)
		{
			positions.ForEach(i =>
			{
				char expectedChar = expectedString[i];
				char actualChar = actualString[i];
				Assert.AreEqual(
					expectedChar,
					actualChar,
					"Characters on the same position both in input string and result stream should be the same!");
			});
		}

		private char ReadCharacter(Stream stream)
		{
			const int startingCharIndex = 0;
			const int charSize = 2;
			byte[] buffer = new byte[charSize];
			stream.Read(buffer, startingCharIndex, charSize);
			char[] chars = Encoding.UTF8.GetChars(buffer);
			return chars[0];
		}

		private int GetDocumentArtifactID(string controlNumber)
		{
			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int) ArtifactType.Document},
				Fields = new[] { new FieldRef { Name = "Artifact ID" } },
				Condition  = $"'Control Number' LIKE '{controlNumber}'))"
			};
			List<RelativityObject> results = _relativityObjectManager.Query(queryRequest);
			return results.First().ArtifactID;
		}

		private IRelativityObjectManager CreateObjectManager()
		{
			var factory = new RelativityObjectManagerFactory(_helper);
			return factory.CreateRelativityObjectManager(_workspaceId);
		}

		private string GetWorkspaceRandomizedName() =>
			$"{_WORKSPACE_NAME}{System.DateTime.UtcNow.ToString(@"yyyy_M_d_hh_mm_ss")}";

	}
}
