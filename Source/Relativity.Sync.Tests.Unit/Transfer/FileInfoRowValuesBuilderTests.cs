﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal sealed class FileInfoRowValuesBuilderTests
	{
		private const long _SIZE = 100;
		private const int _DOCUMENT_ARTIFACT_ID = 10;
		private const string _LOCATION = "abc";
		private const string _FILENAME = "def";

		private const string _INITIAL_VALUE = "ghj";
		private static readonly Type _INITIAL_VALUE_TYPE = _INITIAL_VALUE.GetType();

		public static IEnumerable<TestCaseData> FieldInfoDtos()
		{
			yield return new TestCaseData(FieldInfoDto.DocumentField("abc", "def", false), _INITIAL_VALUE_TYPE, _INITIAL_VALUE);
			yield return new TestCaseData(FieldInfoDto.NativeFileSizeField(), typeof(long), _SIZE);
			yield return new TestCaseData(FieldInfoDto.NativeFileLocationField(), typeof(string), _LOCATION);
			yield return new TestCaseData(FieldInfoDto.NativeFileFilenameField(), typeof(string), _FILENAME);
		}

		[Test]
		[TestCaseSource(nameof(FieldInfoDtos))]
		public void BuildRowValue_ShouldReturnInitialValue_WhenFieldInfoDtoIsDocumentField(FieldInfoDto fieldInfoDto, Type expectedType, object expectedValue)
		{
			// ARRANGE
			RelativityObjectSlim document = new RelativityObjectSlim() { ArtifactID = _DOCUMENT_ARTIFACT_ID };
			IDictionary<int, INativeFile> artifactIdToNativeFileMap = new Dictionary<int, INativeFile>
			{
				{_DOCUMENT_ARTIFACT_ID, new NativeFile(_DOCUMENT_ARTIFACT_ID, _LOCATION, _FILENAME, _SIZE)}
			};

			FileInfoRowValuesBuilder instance = new FileInfoRowValuesBuilder(artifactIdToNativeFileMap);

			// ACT
			object result = instance.BuildRowValue(fieldInfoDto, document, _INITIAL_VALUE);

			// ASSERT
			result.Should().BeOfType(expectedType);
			result.Should().BeEquivalentTo(expectedValue);
		}

		public static IEnumerable<TestCaseData> UnsupportedNonDocumentFieldInfoDtos()
		{
			yield return new TestCaseData(FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure());
		}

		[Test]
		[TestCaseSource(nameof(UnsupportedNonDocumentFieldInfoDtos))]
		public void BuildRowValue_ShouldThrowException_WhenNotSupportedNonDocumentSpecialField(FieldInfoDto fieldInfoDto)
		{
			// ARRANGE
			RelativityObjectSlim document = new RelativityObjectSlim() { ArtifactID = _DOCUMENT_ARTIFACT_ID };
			IDictionary<int, INativeFile> artifactIdToNativeFileMap = new Dictionary<int, INativeFile>
			{
				{_DOCUMENT_ARTIFACT_ID, new NativeFile(_DOCUMENT_ARTIFACT_ID, _LOCATION, _FILENAME, _SIZE)}
			};

			FileInfoRowValuesBuilder instance = new FileInfoRowValuesBuilder(artifactIdToNativeFileMap);

			// ACT
			Action action = () => instance.BuildRowValue(fieldInfoDto, document, _INITIAL_VALUE);

			// ASSERT
			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public void BuildRowValue_ShouldThrowSyncException_WhenDocumentArtifactIdNotPresentInDictionary()
		{
			// ARRANGE
			FieldInfoDto fieldInfoDto = FieldInfoDto.NativeFileSizeField();
			RelativityObjectSlim document = new RelativityObjectSlim() { ArtifactID = _DOCUMENT_ARTIFACT_ID };
			IDictionary<int, INativeFile> artifactIdToNativeFileMap = new Dictionary<int, INativeFile>();

			FileInfoRowValuesBuilder instance = new FileInfoRowValuesBuilder(artifactIdToNativeFileMap);

			// ACT
			Action action = () => instance.BuildRowValue(fieldInfoDto, document, _INITIAL_VALUE);

			// ASSERT
			action.Should().Throw<SyncException>();
		}

		[Test]
		public void BuildRowValue_ShouldThrowSyncException_WhenNativesAreDuplicated()
		{
			// Arrange

			FieldInfoDto fieldInfoDto = FieldInfoDto.NativeFileSizeField();
#pragma warning disable RG2009 // Hardcoded Numeric Value
			IDictionary<int, INativeFile> artifactIdToNativeFileMap = new Dictionary<int, INativeFile>()
			{
				{
					_DOCUMENT_ARTIFACT_ID, new NativeFile(2, string.Empty, string.Empty, 3)
					{
						IsDuplicated = true
					}
				}
			};
#pragma warning restore RG2009 // Hardcoded Numeric Value
			RelativityObjectSlim document = new RelativityObjectSlim() {ArtifactID = _DOCUMENT_ARTIFACT_ID };
			FileInfoRowValuesBuilder instance = new FileInfoRowValuesBuilder(artifactIdToNativeFileMap);

			// Act
			Action action = () => instance.BuildRowValue(fieldInfoDto, document, _INITIAL_VALUE);

			// Assert
			action
				.Should().Throw<SyncException>()
				.Which.Message.Should().Contain($"has more than one native file");
		}
	}
}