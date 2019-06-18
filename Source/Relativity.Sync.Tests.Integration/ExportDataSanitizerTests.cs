﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class ExportDataSanitizerTests
	{
		private Mock<IObjectManager> _objectManager;

		private IContainer _container;

		private const int _ITEM_ARTIFACT_ID = 1012323;
		private const int _SOURCE_WORKSPACE_ID = 1014023;
		private const string _IDENTIFIER_FIELD_NAME = "blech";
		private const string _IDENTIFIER_FIELD_VALUE = "blorgh";
		private const string _SANITIZING_SOURCE_FIELD_NAME = "bar";
		private const string _LONGTEXT_STREAM_SHIBBOLETH = "#KCURA99DF2F0FEB88420388879F1282A55760#";
		private const char _NESTED_DELIM = (char) 29;
		private const char _MULTI_DELIM = (char) 30;

		[SetUp]
		public void InitializeMocks()
		{
			_objectManager = new Mock<IObjectManager>();
			var userServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			userServiceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);

			ContainerBuilder builder = ContainerHelper.CreateInitializedContainerBuilder();
			builder.RegisterInstance(userServiceFactory.Object).As<ISourceServiceFactoryForUser>();
			IntegrationTestsContainerBuilder.MockReporting(builder);
			builder.RegisterInstance(new EmptyLogger()).As<ISyncLog>();

			var configuration = new Mock<ISynchronizationConfiguration>();
			configuration.SetupGet(x => x.ImportSettings).Returns(new ImportSettingsDto());
			builder.RegisterInstance(configuration.Object).As<ISynchronizationConfiguration>();

			builder.RegisterType<ExportDataSanitizer>().As<ExportDataSanitizer>();
			_container = builder.Build();
		}

		private static IEnumerable<TestCaseData> EncodingTestCases()
		{
			yield return new TestCaseData(Encoding.ASCII);
			yield return new TestCaseData(Encoding.Unicode);
		}

		[TestCaseSource(nameof(EncodingTestCases))]
		public async Task LongTextSanitizerShouldReturnSelfDisposingStream(Encoding fieldEncoding)
		{
			// Arrange
			const string sanitizingFieldValue = "this is a test stream";
			SetupStreamTestCase(sanitizingFieldValue, fieldEncoding);

			ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

			// Act
			FieldInfoDto sanitizingSourceField = LongTextField();
			const string initialValue = _LONGTEXT_STREAM_SHIBBOLETH;
			object result = await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, initialValue)
				.ConfigureAwait(false);

			// Assert
			result.Should().NotBeNull().And.BeOfType<SelfDisposingStream>();
		}

		[TestCaseSource(nameof(EncodingTestCases))]
		public async Task LongTextSanitizerShouldReturnUnicodeEncodedStringOnRead(Encoding fieldEncoding)
		{
			// Arrange
			const string sanitizingFieldValue = "this is a test stream";
			SetupStreamTestCase(sanitizingFieldValue, fieldEncoding);

			ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

			// Act
			FieldInfoDto sanitizingSourceField = LongTextField();
			const string initialValue = _LONGTEXT_STREAM_SHIBBOLETH;
			object result = await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, initialValue)
				.ConfigureAwait(false);

			// Assert
			Stream resultStream = (Stream)result;

			const int bufferLength = 1024;
			byte[] streamedResultBuffer = new byte[bufferLength];
			int bytesRead = resultStream.Read(streamedResultBuffer, 0, streamedResultBuffer.Length);
			string reconstructedString = string.Join("", Encoding.Unicode.GetChars(streamedResultBuffer, 0, bytesRead));
			reconstructedString.Should().Be(sanitizingFieldValue);
		}

		private static IEnumerable<TestCaseData> ObjectChoiceSanitizerGoldenTestCases()
		{
			yield return new TestCaseData(RelativityDataType.SingleObject, null, null);
			yield return new TestCaseData(RelativityDataType.SingleObject, JsonHelpers.ToJToken<JObject>(new RelativityObjectValue { Name = "FooBar" }), "FooBar");

			yield return new TestCaseData(RelativityDataType.SingleChoice, null, null);
			yield return new TestCaseData(RelativityDataType.SingleChoice, JsonHelpers.ToJToken<JObject>(new Choice { Name = "FooBar" }), "FooBar");

			yield return new TestCaseData(RelativityDataType.MultipleObject, null, null);
			yield return new TestCaseData(
				RelativityDataType.MultipleObject,
				ObjectValueJArrayFromNames("Test Name", "Cool Name", "Rad Name"),
				$"Test Name{_MULTI_DELIM}Cool Name{_MULTI_DELIM}Rad Name");

			yield return new TestCaseData(RelativityDataType.MultipleChoice, null, null);
			yield return new TestCaseData(
				RelativityDataType.MultipleChoice,
				ChoiceJArrayFromNames("Test Name", "Cool Name", "Rad Name"),
				$"Test Name{_MULTI_DELIM}Cool Name{_MULTI_DELIM}Rad Name");

			// TODO: Add tests for nested multiple choice values
		}

		[TestCaseSource(nameof(ObjectChoiceSanitizerGoldenTestCases))]
		public async Task ObjectChoiceSanitizersShouldConstructCorrectValue(RelativityDataType type, object initialValue, object expectedResult)
		{
			// Arrange
			ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

			// Act
			FieldInfoDto sanitizingSourceField = DefaultField();
			sanitizingSourceField.RelativityDataType = type;
			object result = await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, initialValue)
				.ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedResult);
		}

		[Test]
		public async Task MultipleObjectSanitizerShouldThrowOnInvalidObjectName()
		{
			// Arrange
			ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

			// Act
			FieldInfoDto sanitizingSourceField = DefaultField();
			sanitizingSourceField.RelativityDataType = RelativityDataType.MultipleObject;
			object initialValue = ObjectValueJArrayFromNames("Cool Name", $"Test{_MULTI_DELIM} Name", "Other Name", $"This{_NESTED_DELIM} Guy", $"Some{_MULTI_DELIM}{_MULTI_DELIM} Other");
			Func<Task> action = async () => await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, initialValue)
				.ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
					.MatchRegex($": 'Test{_MULTI_DELIM} Name', 'Some{_MULTI_DELIM}{_MULTI_DELIM} Other'$").And
					.Contain(sanitizingSourceField.SourceFieldName);
		}

		[Test]
		public async Task MultipleChoiceSanitizerShouldThrowOnInvalidChoiceName()
		{
			// Arrange
			ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

			// Act
			FieldInfoDto sanitizingSourceField = DefaultField();
			sanitizingSourceField.RelativityDataType = RelativityDataType.MultipleChoice;
			object initialValue = ChoiceJArrayFromNames("Cool Name", $"Test{_MULTI_DELIM} Name", "Other Name", $"This{_NESTED_DELIM} Guy", $"Some{_MULTI_DELIM}{_MULTI_DELIM} Other");
			Func<Task> action = async () => await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, initialValue)
				.ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
				.MatchRegex($": 'Test{_MULTI_DELIM} Name', 'This{_NESTED_DELIM} Guy', 'Some{_MULTI_DELIM}{_MULTI_DELIM} Other'$").And
				.Contain(sanitizingSourceField.SourceFieldName);
		}

		private void SetupStreamTestCase(string value, Encoding fieldEncoding)
		{
			QueryResultSlim itemArtifactResult = WrapArtifactIdInQueryResultSlim(_ITEM_ARTIFACT_ID);
			SetupItemArtifactIdRequest(MatchAll).ReturnsAsync(itemArtifactResult);

			bool isUnicode = fieldEncoding is UnicodeEncoding;
			QueryResultSlim fieldEncodingResult = WrapValuesInQueryResultSlim(isUnicode);
			SetupFieldEncodingRequest(MatchAll).ReturnsAsync(fieldEncodingResult);

			byte[] streamValueBytes = fieldEncoding.GetBytes(value);

			Mock<IKeplerStream> keplerStream = new Mock<IKeplerStream>();
			keplerStream.Setup(x => x.GetStreamAsync()).ReturnsAsync(new MemoryStream(streamValueBytes));
			SetupStreamLongText(MatchAll, MatchAll)
				.ReturnsAsync(keplerStream.Object);
		}

		private static FieldInfoDto DefaultField()
		{
			FieldInfoDto field = FieldInfoDto.DocumentField(_SANITIZING_SOURCE_FIELD_NAME, "blah", false);
			return field;
		}

		private static FieldInfoDto LongTextField()
		{
			FieldInfoDto field = FieldInfoDto.DocumentField(_SANITIZING_SOURCE_FIELD_NAME, "blah", false);
			field.RelativityDataType = RelativityDataType.LongText;
			return field;
		}

		private static bool MatchAll(object _) => true;

		// The next two methods do need to check ArtifactTypeId/Name by default so that we can differentiate between the two OM calls.

		private ISetup<IObjectManager, Task<QueryResultSlim>> SetupItemArtifactIdRequest(Func<QueryRequest, bool> queryMatcher)
		{
			return _objectManager.Setup(x => x.QuerySlimAsync(
				It.IsAny<int>(),
				It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int)ArtifactType.Document && queryMatcher(q)),
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

		private static JArray ObjectValueJArrayFromNames(params string[] names)
		{
			RelativityObjectValue[] objectValues = names.Select(x => new RelativityObjectValue { Name = x }).ToArray();
			return JsonHelpers.ToJToken<JArray>(objectValues);
		}

		private static JArray ChoiceJArrayFromNames(params string[] names)
		{
			Choice[] choices = names.Select(x => new Choice { Name = x }).ToArray();
			return JsonHelpers.ToJToken<JArray>(choices);
		}
	}
}
