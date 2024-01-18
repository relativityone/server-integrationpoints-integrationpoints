using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    internal sealed class SourceWorkspaceDataReaderMetadataTests : SourceWorkspaceDataReaderTestsBase
    {
        [Test]
        public async Task Read_ShouldReturnLongTextStream_WhenGivenShibboleth()
        {
            // Arrange
            const int batchSize = 100;
            SetUp(batchSize);

            const string columnName = "LongText";
            const string bigTextShibboleth = "#KCURA99DF2F0FEB88420388879F1282A55760#";

            HashSet<FieldConfiguration> fields = DefaultIdentifierWithSpecialFields;
            fields.Add(FieldConfiguration.Regular(columnName, columnName, RelativityDataType.LongText, bigTextShibboleth));

            DocumentImportJob importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForNativesTransfer, fields);
            _configuration.SetFieldMappings(importData.FieldMappings);

            await _documentTransferServicesMocker.SetupServicesWithNativesTestDataAsync(importData, batchSize).ConfigureAwait(false);

            Encoding encoding = Encoding.Unicode;
            const string expectedStreamContent = "Hello world!";
            _documentTransferServicesMocker.SetupLongTextStream(columnName, encoding, expectedStreamContent);

            // Act
            _instance.Read();
            int columnIndex = _instance.GetOrdinal(columnName);
            object actualValue = _instance.GetValue(columnIndex);

            // Assert
            var streamValue = actualValue as Stream;
            streamValue.Should().NotBeNull();

            var streamReader = new StreamReader(streamValue, encoding);
            string streamContents = streamReader.ReadToEnd();

            streamContents.Should().Be(expectedStreamContent);
        }

        [Test]
        public async Task Read_ShouldReturnCorrectMultipleChoiceTree()
        {
            // Arrange
            const int batchSize = 100;
            SetUp(batchSize);

            dynamic[] choiceValues =
            {
                new { Parent = 0, ArtifactID = 1, Name = "Foo" },
                new { Parent = 1, ArtifactID = 3, Name = "Bar" },
                new { Parent = 3, ArtifactID = 5, Name = "Baz" },
                new { Parent = 1, ArtifactID = 4, Name = "Bat" },
                new { Parent = 0, ArtifactID = 2, Name = "Bang" }
            };

            SetupObjectManagerForMultipleChoiceTree(choiceValues, _documentTransferServicesMocker.ObjectManager);

            List<dynamic> convertedForExport = choiceValues
                .Select(v => new { v.ArtifactID, v.Name })
                .Cast<dynamic>()
                .ToList();

            object inputValue = RunThroughSerializer(convertedForExport);
            const string columnName = "MultipleChoice";

            HashSet<FieldConfiguration> fields = DefaultIdentifierWithSpecialFields;
            fields.Add(FieldConfiguration.Regular(columnName, columnName, RelativityDataType.MultipleChoice, inputValue));

            DocumentImportJob importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForNativesTransfer, fields);
            _configuration.SetFieldMappings(importData.FieldMappings);

            await _documentTransferServicesMocker.SetupServicesWithNativesTestDataAsync(importData, batchSize).ConfigureAwait(false);

            // Act
            _instance.Read();
            int columnIndex = _instance.GetOrdinal(columnName);
            object actualValue = _instance.GetValue(columnIndex);

            // Assert
            const char mult = LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII;
            const char nest = LoadFileOptions._DEFAULT_NESTED_VALUE_ASCII;
            string expectedValue = $"Foo{nest}Bar{nest}Baz{mult}Foo{nest}Bat{mult}Bang{mult}";
            actualValue.Should().Be(expectedValue);
        }

        private void SetupObjectManagerForMultipleChoiceTree(dynamic[] values, Mock<IObjectManager> objectManager)
        {
            HashSet<int> registered = new HashSet<int>();
            foreach (dynamic value in values)
            {
                if (!registered.Contains(value.ArtifactID))
                {
                    int artifactId = value.ArtifactID;
                    int parentArtifactId = value.Parent;
                    objectManager
                        .Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(r => r.Condition == $"'Artifact ID' == {artifactId}"), It.IsAny<int>(), It.IsAny<int>()))
                        .ReturnsAsync(new QueryResult { Objects = new List<RelativityObject> { new RelativityObject { ParentObject = new RelativityObjectRef { ArtifactID = parentArtifactId } } } });

                    registered.Add(value.ArtifactID);
                }
            }
        }

        protected override IBatchDataReaderBuilder CreateBatchDataReaderBuilder()
        {
            return new NativeBatchDataReaderBuilder(_container.Resolve<IFieldManager>(), _container.Resolve<IExportDataSanitizer>(), new EmptyLogger());
        }
    }
}
