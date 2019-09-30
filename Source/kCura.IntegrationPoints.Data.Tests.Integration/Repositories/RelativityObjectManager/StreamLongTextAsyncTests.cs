﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using ArtifactType = Relativity.ArtifactType;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories.RelativityObjectManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class StreamLongTextAsyncTests
	{
		private int _workspaceId;
		private IHelper _helper;
		private IRelativityObjectManager _relativityObjectManager;
		private ImportHelper _importHelper;
		private WorkspaceService _workspaceService;

		private const string _WORKSPACE_NAME = "RIPStreamLongTextTests";
		private const string _EXTRACTED_TEXT_FIELD_NAME = "Extracted Text";

		private readonly string[] _CONTROL_NUMBERS_POOL =
			new[]
			{
				"9b2d388d08974c02a3ca5fe768e7e234",
				"d32b43508b574a1eb06523d4e1cdeb25"
			};

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string workspaceName = GetWorkspaceRandomizedName();
			_workspaceId = Workspace.CreateWorkspace(workspaceName);
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

		[IdentifiedTest("be26fb26-a1c5-401d-84ba-57122d8c6e75")]
		public void ItShouldFetchDocumentWith15MBExtractedText()
		{
			int bytes = GetBytesFromMB(15);
			string controlNumber = _CONTROL_NUMBERS_POOL[0];
			ExecuteTest(bytes, controlNumber);
		}

		//This test has been changed from 1500MB to 200MB
		//Change it back to 1500MB after IAPI removes 10 minutes timeout per item in import
		//JIRA: REL-358031
		[StressTest]
		[IdentifiedTest("d1e9a3e4-943a-42aa-993e-9d8933883a1e")]
		public void ItShouldFetchDocumentWith200MBExtractedText()
		{
			int bytes = GetBytesFromMB(200);
			string controlNumber = _CONTROL_NUMBERS_POOL[1];
			ExecuteTest(bytes, controlNumber);
		}

		private int GetBytesFromMB(int sizeInMB) => sizeInMB * 1024 * 1024;

		private void ExecuteTest(int textSizeInBytes, string controlNumber)
		{
			string filePath = "";
			try
			{
				// Arrange
				string extractedText = DocumentTestDataBuilder.GenerateRandomExtractedText(textSizeInBytes);

				string fileName = $"{controlNumber}.txt";
				string assemblyDir =
					Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
						.Replace(@"file:\", "");
				filePath = Path.Combine(assemblyDir, fileName);
				File.WriteAllText(filePath, extractedText, Encoding.UTF8);

				_workspaceService.ImportSingleExtractedText(_workspaceId, controlNumber, filePath);
				int documentArtifactID = GetDocumentArtifactID(controlNumber);

				// Act
				Stream actualExtractedTextStream =
					_relativityObjectManager.StreamUnicodeLongText(
						documentArtifactID,
						new FieldRef
						{
							Name = _EXTRACTED_TEXT_FIELD_NAME
						}
					);
				var actualExtractedTextStreamReader = new StreamReader(actualExtractedTextStream, Encoding.UTF8);
				string actualExtractedTextString = actualExtractedTextStreamReader.ReadToEnd();

				// Assert
				extractedText.Length
					.Should()
					.Be(
						actualExtractedTextString.Length,
						"Extracted Text returned by ObjectManager should be the same length as original text!");

				IEnumerable<int> charsIndexes = GetExponentialIndexes(extractedText.Length);
				ValidateSpecificCharacters(charsIndexes, extractedText, actualExtractedTextString);

			}
			finally
			{
				// Tear Down
				if (!string.IsNullOrWhiteSpace(filePath))
				{
					File.Delete(filePath);
				}
			}
		}

		private int[] GetExponentialIndexes(int size)
		{
			int @base = 2;
			int lastExponent = (int)Math.Floor(Math.Log(size, @base));
			return new int[] { 0 }
				.Concat(Enumerable.Range(0, lastExponent))
				.Select(x => (int)Math.Pow(@base, x))
				.Concat(new int[] { size - 1 })
				.ToArray();
		}

		private void ValidateSpecificCharacters(
			IEnumerable<int> positions,
			string expectedString,
			string actualString)
		{
			foreach (int position in positions)
			{
				char expectedChar = expectedString[position];
				char actualChar = actualString[position];
				expectedChar
					.Should()
					.Be(actualChar,
						"Characters on the same position both in input string and result stream should be the same!");
			}
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
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
				Fields = new[] { new FieldRef { Name = "Artifact ID" } },
				Condition = $"'Control Number' LIKE '{controlNumber}'))"
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
