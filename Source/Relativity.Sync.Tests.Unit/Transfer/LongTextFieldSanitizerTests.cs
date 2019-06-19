﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	[Parallelizable(ParallelScope.Self)]
	internal sealed class LongTextFieldSanitizerTests
	{
		private Mock<IObjectManager> _objectManager;
		private Mock<ISourceServiceFactoryForUser> _userServiceFactory;
		private Mock<ISyncLog> _logger;
		private Mock<IImportStreamBuilder> _streamBuilder;

		private const int _ITEM_ARTIFACT_ID = 1012323;
		private const int _SOURCE_WORKSPACE_ID = 1014023;
		private const string _IDENTIFIER_FIELD_NAME = "blech";
		private const string _IDENTIFIER_FIELD_VALUE = "blorgh";
		private const string _SANITIZING_SOURCE_FIELD_NAME = "bar";
		private const string _LONGTEXT_STREAM_SHIBBOLETH = "#KCURA99DF2F0FEB88420388879F1282A55760#";

		[SetUp]
		public void InitializeMocks()
		{
			_objectManager = new Mock<IObjectManager>();
			_userServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			_userServiceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);
			_streamBuilder = new Mock<IImportStreamBuilder>();
			_logger = new Mock<ISyncLog>();
		}

		[Test]
		public void ItShouldSupportLongText()
		{
			// Arrange
			var instance = new LongTextFieldSanitizer(_userServiceFactory.Object, _streamBuilder.Object, _logger.Object);

			// Act
			RelativityDataType supportedType = instance.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.LongText);
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase("foo")]
		[TestCase("#KCURA99DF2F0FEB88420388879F1282A55760")]
		public async Task ItShouldReturnNonShibbolethStringInitialValue(object initialValue)
		{
			// Arrange
			var instance = new LongTextFieldSanitizer(_userServiceFactory.Object, _streamBuilder.Object, _logger.Object);

			// Act
			object result = await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, _SANITIZING_SOURCE_FIELD_NAME, initialValue)
				.ConfigureAwait(false);

			// Assert
			result.Should().Be(initialValue);
		}

		[Test]
		public async Task ItShouldThrowWhenGivenNonStringInitialValue()
		{
			// Arrange
			var instance = new LongTextFieldSanitizer(_userServiceFactory.Object, _streamBuilder.Object, _logger.Object);

			// Act
			DateTime initialValue = DateTime.Now;
			Func<Task> action = async () => await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, _SANITIZING_SOURCE_FIELD_NAME, initialValue)
				.ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false))
				.Which.Message.Should().Contain(typeof(DateTime).FullName);
		}

		[Test]
		public async Task ItShouldUseCorrectValuesForObjectManagerQueries()
		{
			// This is tying the implementation pretty closely to the unit tests, but
			// if we limit query-specific assertions to this test then we can still
			// check basic correctness without making the tested class a PITA to refactor.

			// Arrange
			const int itemArtifactId = _ITEM_ARTIFACT_ID;
			const int workspaceArtifactId = _SOURCE_WORKSPACE_ID;
			const string itemIdentifierSourceFieldName = _IDENTIFIER_FIELD_NAME;
			const string itemIdentifier = _IDENTIFIER_FIELD_VALUE;
			const string sanitizingSourceFieldName = _SANITIZING_SOURCE_FIELD_NAME;

			QueryResultSlim itemArtifactResult = WrapArtifactIdInQueryResultSlim(itemArtifactId);
			SetupItemArtifactIdRequest(q => q.Condition.Contains($"'{itemIdentifierSourceFieldName}' == '{itemIdentifier}'"))
				.ReturnsAsync(itemArtifactResult)
				.Verifiable();

			const bool isUnicode = true;
			QueryResultSlim fieldEncodingResult = WrapValuesInQueryResultSlim(isUnicode);
			SetupFieldEncodingRequest(q => q.Condition.Contains($"'Name' == '{sanitizingSourceFieldName}'"))
				.ReturnsAsync(fieldEncodingResult)
				.Verifiable();

			SetupStreamBuilder().Returns<Func<Stream>, StreamEncoding>((f, _) => f());

			string sanitizingFieldValue = string.Empty;
			byte[] streamValueBytes = Encoding.Unicode.GetBytes(sanitizingFieldValue);

			Mock<IKeplerStream> keplerStream = new Mock<IKeplerStream>();
			keplerStream.Setup(x => x.GetStreamAsync()).ReturnsAsync(new MemoryStream(streamValueBytes));
			SetupStreamLongText(objectRef => objectRef.ArtifactID == itemArtifactId, fieldRef => fieldRef.Name == sanitizingSourceFieldName)
				.ReturnsAsync(keplerStream.Object)
				.Verifiable();

			var instance = new LongTextFieldSanitizer(_userServiceFactory.Object, _streamBuilder.Object, _logger.Object);

			// Act
			const string initialValue = _LONGTEXT_STREAM_SHIBBOLETH;
			await instance.SanitizeAsync(workspaceArtifactId, itemIdentifierSourceFieldName, itemIdentifier, sanitizingSourceFieldName, initialValue)
				.ConfigureAwait(false);

			// Assert
			_objectManager.Verify();
		}

		private static IEnumerable<TestCaseData> EncodingTestCases()
		{
			yield return new TestCaseData(StreamEncoding.ASCII);
			yield return new TestCaseData(StreamEncoding.Unicode);
		}

		[TestCaseSource(nameof(EncodingTestCases))]
		public async Task ItShouldLogErrorAndRethrowWhenStreamCreationThrows(StreamEncoding fieldEncoding)
		{
			// Arrange
			const string sanitizingFieldName = _SANITIZING_SOURCE_FIELD_NAME;

			QueryResultSlim itemArtifactResult = WrapArtifactIdInQueryResultSlim(_ITEM_ARTIFACT_ID);
			SetupItemArtifactIdRequest(MatchAll).ReturnsAsync(itemArtifactResult);

			bool isUnicode = fieldEncoding == StreamEncoding.Unicode;
			QueryResultSlim fieldEncodingResult = WrapValuesInQueryResultSlim(isUnicode);
			SetupFieldEncodingRequest(MatchAll).ReturnsAsync(fieldEncodingResult);

			SetupStreamBuilder().Throws<ServiceException>();

			var instance = new LongTextFieldSanitizer(_userServiceFactory.Object, _streamBuilder.Object, _logger.Object);

			// Act
			const string initialValue = _LONGTEXT_STREAM_SHIBBOLETH;
			Func<Task> action = async () => await instance.SanitizeAsync(0, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingFieldName, initialValue)
				.ConfigureAwait(false);

			// Assert
			await action.Should().ThrowAsync<ServiceException>().ConfigureAwait(false);

			string expectedEncoding = fieldEncoding.ToString();
			_logger.Verify(x => x.LogError(
				It.IsAny<Exception>(),
				It.Is<string>(y => y.Contains(sanitizingFieldName) && y.Contains(expectedEncoding))));
		}

		[TestCaseSource(nameof(EncodingTestCases))]
		public async Task ItShouldPassThroughCorrectStreamEncoding(StreamEncoding fieldEncoding)
		{
			// Arrange
			QueryResultSlim itemArtifactResult = WrapArtifactIdInQueryResultSlim(_ITEM_ARTIFACT_ID);
			SetupItemArtifactIdRequest(MatchAll).ReturnsAsync(itemArtifactResult);

			bool isUnicode = fieldEncoding == StreamEncoding.Unicode;
			QueryResultSlim fieldEncodingResult = WrapValuesInQueryResultSlim(isUnicode);
			SetupFieldEncodingRequest(MatchAll).ReturnsAsync(fieldEncodingResult);

			SetupStreamBuilder().Returns(new MemoryStream());

			var instance = new LongTextFieldSanitizer(_userServiceFactory.Object, _streamBuilder.Object, _logger.Object);

			// Act
			const string initialValue = _LONGTEXT_STREAM_SHIBBOLETH;
			await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, _SANITIZING_SOURCE_FIELD_NAME, initialValue)
				.ConfigureAwait(false);

			// Assert
			//_streamBuilder.Verify(x => x.Create(It.IsAny<Func<Stream>>(), fieldEncoding));
			Assert.Fail("To be adjusted to new implementation");
		}

		private static bool MatchAll(object _) => true;

		// The next two methods do need to check ArtifactTypeId/Name by default so that we can differentiate between the two OM calls.

		private ISetup<IObjectManager, Task<QueryResultSlim>> SetupItemArtifactIdRequest(Func<QueryRequest, bool> queryMatcher)
		{
			return _objectManager.Setup(x => x.QuerySlimAsync(
				It.IsAny<int>(),
				It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int) ArtifactType.Document && queryMatcher(q)),
				It.IsAny<int>(),
				It.IsAny<int>()));
		}

		private ISetup<IObjectManager, Task<QueryResultSlim>> SetupFieldEncodingRequest(Func<QueryRequest, bool> queryMatcher)
		{
			return _objectManager.Setup(x => x.QuerySlimAsync(
				It.IsAny<int>(),
				It.Is<QueryRequest>(q => q.ObjectType.Name == "Field" && queryMatcher(q)),
				It.IsAny<int>(),
				It.IsAny<int>()));
		}

		private ISetup<IObjectManager, Task<IKeplerStream>> SetupStreamLongText(
			Func<RelativityObjectRef, bool> objectRefMatcher,
			Func<FieldRef, bool> fieldRefMatcher)
		{
			return _objectManager.Setup(x => x.StreamLongTextAsync(
				It.IsAny<int>(),
				It.Is<RelativityObjectRef>(r => objectRefMatcher(r)),
				It.Is<FieldRef>(r => fieldRefMatcher(r))));
		}

		private ISetup<IImportStreamBuilder, Stream> SetupStreamBuilder()
		{
			//return _streamBuilder.Setup(x => x.Create(It.IsAny<Func<Stream>>(), It.IsAny<StreamEncoding>()));
			Assert.Fail("To be adjusted to new implementation");
			return null; // to be deleted after fixing test
		}

		private static QueryResultSlim WrapValuesInQueryResultSlim(params object[] values)
		{
			return new QueryResultSlim
			{
				Objects = new List<RelativityObjectSlim>
				{
					new RelativityObjectSlim { Values = values.ToList() }
				}
			};
		}

		private static QueryResultSlim WrapArtifactIdInQueryResultSlim(int artifactId)
		{
			return new QueryResultSlim
			{
				Objects = new List<RelativityObjectSlim>
				{
					new RelativityObjectSlim {ArtifactID = artifactId}
				}
			};
		}
	}
}
