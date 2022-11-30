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
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    internal sealed class ExportDataSanitizerTests
    {
        private Mock<IObjectManager> _objectManager;

        private IContainer _container;

        private const int _CHOICE_ARTIFACT_TYPE_ID = 7;
        private const int _ITEM_ARTIFACT_ID = 1012323;
        private const int _SOURCE_WORKSPACE_ID = 1014023;
        private const string _IDENTIFIER_FIELD_NAME = "blech";
        private const string _IDENTIFIER_FIELD_VALUE = "blorgh";
        private const string _SANITIZING_SOURCE_FIELD_NAME = "bar";
        private const string _LONGTEXT_STREAM_SHIBBOLETH = "#KCURA99DF2F0FEB88420388879F1282A55760#";
        private const char _NESTED_DELIM = (char)29;
        private const char _MULTI_DELIM = (char)30;

        [SetUp]
        public void InitializeMocks()
        {
            _objectManager = new Mock<IObjectManager>();
            var serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            var serviceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
            serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManager.Object);
            serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManager.Object);

            ContainerBuilder builder = ContainerHelper.CreateInitializedContainerBuilder();
            builder.RegisterInstance(serviceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();
            builder.RegisterInstance(serviceFactoryForAdmin.Object).As<ISourceServiceFactoryForAdmin>();
            IntegrationTestsContainerBuilder.MockReportingWithProgress(builder);
            builder.RegisterInstance(new EmptyLogger()).As<IAPILog>();

            var configuration = new ConfigurationStub();
            builder.RegisterInstance(configuration).AsImplementedInterfaces();

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
            string reconstructedString = string.Join(string.Empty, Encoding.Unicode.GetChars(streamedResultBuffer, 0, bytesRead));
            reconstructedString.Should().Be(sanitizingFieldValue);
        }

        private static IEnumerable<TestCaseData> ObjectChoiceSanitizerGoldenTestCases()
        {
            yield return new TestCaseData(RelativityDataType.SingleObject, null, null);
            yield return new TestCaseData(RelativityDataType.SingleObject, JsonHelpers.ToJToken<JObject>(new RelativityObjectValue { ArtifactID = 1, Name = "FooBar" }), "FooBar");

            yield return new TestCaseData(RelativityDataType.SingleChoice, null, null);
            yield return new TestCaseData(RelativityDataType.SingleChoice, JsonHelpers.ToJToken<JObject>(new Choice { Name = "FooBar" }), "FooBar");

            yield return new TestCaseData(RelativityDataType.MultipleObject, null, null);
            yield return new TestCaseData(
                RelativityDataType.MultipleObject,
                ObjectValueJArrayFromNames("Test Name", "Cool Name", "Rad Name"),
                $"Test Name{_MULTI_DELIM}Cool Name{_MULTI_DELIM}Rad Name");

            yield return new TestCaseData(RelativityDataType.MultipleChoice, null, null);
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
        public async Task ItShouldSanitizeMultipleChoice()
        {
#pragma warning disable RG2009 // Hardcoded Numeric Value - using a lot of hardoced values here
            // Arrange
            ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

            Choice[] choices =
            {
                new Choice()
                {
                    ArtifactID = 1,
                    Name = "Test Name"
                },
                new Choice()
                {
                    ArtifactID = 2,
                    Name = "Cool Name"
                },
                new Choice()
                {
                    ArtifactID = 3,
                    Name = "Rad Name"
                },
            };

            foreach (Choice choice in choices)
            {
                QueryResult queryResult = new QueryResult()
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            ArtifactID = choice.ArtifactID,
                            Name = choice.Name,
                            ParentObject = new RelativityObjectRef()
                        }
                    }
                };
                _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(r =>
                        r.ObjectType.ArtifactTypeID == _CHOICE_ARTIFACT_TYPE_ID), It.IsAny<int>(), It.IsAny<int>()))
                        .ReturnsAsync(queryResult);
            }

            string expectedResult = $"Test Name{_MULTI_DELIM}Cool Name{_MULTI_DELIM}Rad Name{_MULTI_DELIM}";

            // Act
            FieldInfoDto sanitizingSourceField = DefaultField();
            sanitizingSourceField.RelativityDataType = RelativityDataType.MultipleChoice;
            object result = await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, ChoiceJArrayFromChoices(choices))
                .ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedResult);
#pragma warning restore RG2009 // Hardcoded Numeric Value
        }

        [Test]
        public async Task ItShouldSanitizeNestedMultipleChoice()
        {
#pragma warning disable RG2009 // Hardcoded Numeric Value - using a lot of hardoced values here
            // Arrange
            ExportDataSanitizer instance = _container.Resolve<ExportDataSanitizer>();

            const int parentArtifactId = 1;
            const int nestedArtifactId = 2;
            Choice parentChoice = new Choice()
            {
                ArtifactID = parentArtifactId,
                Name = "Parent Name"
            };
            Choice nestedChoice = new Choice()
            {
                ArtifactID = nestedArtifactId,
                Name = "Nested Name"
            };
            Choice[] choices =
            {
                parentChoice,
                nestedChoice,
            };

            QueryResult queryResultForParent = new QueryResult
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject
                    {
                        ArtifactID = parentChoice.ArtifactID,
                        Name = parentChoice.Name,
                        ParentObject = new RelativityObjectRef()
                    }
                }
            };
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(r =>
                    r.Condition == $"'Artifact ID' == {parentArtifactId}"), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(queryResultForParent);

            QueryResult queryResultForNested = new QueryResult
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject
                    {
                        ArtifactID = nestedChoice.ArtifactID,
                        Name = nestedChoice.Name,
                        ParentObject = new RelativityObjectRef()
                        {
                            ArtifactID = parentArtifactId
                        }
                    }
                }
            };
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(r =>
                    r.Condition == $"'Artifact ID' == {nestedArtifactId}"), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(queryResultForNested);

            string expectedResult = $"{parentChoice.Name}{_NESTED_DELIM}{nestedChoice.Name}{_MULTI_DELIM}";

            // Act
            FieldInfoDto sanitizingSourceField = DefaultField();
            sanitizingSourceField.RelativityDataType = RelativityDataType.MultipleChoice;
            object result = await instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, ChoiceJArrayFromChoices(choices))
                .ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedResult);
#pragma warning restore RG2009 // Hardcoded Numeric Value
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
            Func<Task> action = () => instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, initialValue);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                    .Be("Unable to parse data from Relativity Export API: " +
                        "The identifiers of the objects in Multiple Object field contain the character specified as the multi-value delimiter ('ASCII 30'). " +
                        "Rename these objects to not contain delimiter.");
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
            Func<Task> action = () => instance.SanitizeAsync(_SOURCE_WORKSPACE_ID, _IDENTIFIER_FIELD_NAME, _IDENTIFIER_FIELD_VALUE, sanitizingSourceField, initialValue);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                .Be("Unable to parse data from Relativity Export API: " +
                    "The identifiers of the choices contain the character specified as the multi-value delimiter ('ASCII 30') or " +
                    "nested value delimiter ('ASCII 29'). Rename choices to not contain delimiters.");
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
                    new RelativityObjectSlim { ArtifactID = artifactId }
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
            return ChoiceJArrayFromChoices(choices);
        }

        private static JArray ChoiceJArrayFromChoices(params Choice[] choices)
        {
            return JsonHelpers.ToJToken<JArray>(choices);
        }
    }
}
