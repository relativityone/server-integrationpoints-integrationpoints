using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class ExportDataSanitizerTests
	{
		private Mock<IObjectManager> _objectManager;
		private Mock<ISyncLog> _logger;
		private FieldInfoDto _sanitizingSourceField;

		private IContainer _container;

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
			var userServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			userServiceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);
			_logger = new Mock<ISyncLog>();

			_sanitizingSourceField = FieldInfoDto.DocumentField(_SANITIZING_SOURCE_FIELD_NAME, "blah", false);
			_sanitizingSourceField.RelativityDataType = RelativityDataType.LongText;

			ContainerBuilder builder = ContainerHelper.CreateInitializedContainerBuilder();
			builder.RegisterInstance(userServiceFactory.Object).As<ISourceServiceFactoryForUser>();
			IntegrationTestsContainerBuilder.MockReporting(builder);
			builder.RegisterInstance(_logger.Object).As<ISyncLog>();

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
			SetupSuccessfulStreamTestCase(sanitizingFieldValue, fieldEncoding);

			ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

			// Act
			const string initialValue = _LONGTEXT_STREAM_SHIBBOLETH;
			object result = await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, _sanitizingSourceField, initialValue)
				.ConfigureAwait(false);

			// Assert
			result.Should().NotBeNull().And.BeOfType<SelfDisposingStream>();
		}

		[TestCaseSource(nameof(EncodingTestCases))]
		public async Task LongTextSanitizerShouldReturnUnicodeEncodedStringOnRead(Encoding fieldEncoding)
		{
			// Arrange
			const string sanitizingFieldValue = "this is a test stream";
			SetupSuccessfulStreamTestCase(sanitizingFieldValue, fieldEncoding);

			ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

			// Act
			const string initialValue = _LONGTEXT_STREAM_SHIBBOLETH;
			object result = await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, _sanitizingSourceField, initialValue)
				.ConfigureAwait(false);

			// Assert
			Stream resultStream = (Stream)result;

			const int bufferLength = 1024;
			byte[] streamedResultBuffer = new byte[bufferLength];
			int bytesRead = resultStream.Read(streamedResultBuffer, 0, streamedResultBuffer.Length);
			string reconstructedString = string.Join("", Encoding.Unicode.GetChars(streamedResultBuffer, 0, bytesRead));
			reconstructedString.Should().Be(sanitizingFieldValue);
		}

		private void SetupSuccessfulStreamTestCase(string value, Encoding fieldEncoding)
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
	}
}
