using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Antlr.Runtime.Misc;
using AutoFixture;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.CustomProviderHelpers;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Core.Storage;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.MyFirstProvider.Provider;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class LoadFileBuilderTests
    {
        private Mock<IRelativityStorageService> _storageServiceFake;
        private Mock<IEntityFullNameService> _entityFullNameServiceFake;
        private IDataSourceProvider _sourceProvider;

        private LoadFileBuilder _sut;

        private IFixture _fxt;

        private string _dataFilePath;
        private string _dataIdsFilePath;
        private string _importDirectory;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _importDirectory = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), _fxt.Create<string>())).FullName;

            _dataFilePath = Path.Combine(_importDirectory, Path.GetTempFileName());
            _dataIdsFilePath = Path.Combine(_importDirectory, Path.GetTempFileName());

            SetupStorageService();

            _entityFullNameServiceFake = new Mock<IEntityFullNameService>();
            _entityFullNameServiceFake.Setup(
                    x => x.FormatFullName(It.IsAny<Dictionary<string, IndexedFieldMap>>(), It.IsAny<IDataReader>()))
                .Returns(_fxt.Create<string>());

            _sourceProvider = new MyFirstProvider();

            _sut = new LoadFileBuilder(
                _storageServiceFake.Object,
                _entityFullNameServiceFake.Object,
                Mock.Of<IAPILog>());
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_importDirectory))
            {
                Directory.Delete(_importDirectory, true);
            }
        }

        [Test]
        public async Task CreateDataFileAsync_ShouldReturnSettingsWithWrittenFile_WhenFullNameIsNotMapped()
        {
            // Arrange
            const int recordsCount = 2500;

            string[] fields = MyFirstProviderXmlGenerator.DefaultColumns;
            string identifier = fields.First();

            string xmlData = MyFirstProviderXmlGenerator.GenerateRecords(recordsCount, fields);
            File.WriteAllText(_dataFilePath, xmlData);

            List<IndexedFieldMap> fieldsMapping = PrepareMapping(fields, identifier);

            IntegrationPointInfo integrationPointInfo = _fxt.Build<IntegrationPointInfo>()
                .With(x => x.FieldMap, fieldsMapping)
                .With(x => x.SourceConfiguration, _dataFilePath)
                .Create();

            SetupBatchIdsFile(fieldsMapping);

            CustomProviderBatch batch = _fxt.Build<CustomProviderBatch>()
                .With(x => x.IDsFilePath, _dataIdsFilePath)
                .Create();

            // Act
            DataSourceSettings result = await _sut.CreateDataFileAsync(batch, _sourceProvider, integrationPointInfo, _importDirectory).ConfigureAwait(false);

            // Assert
            string[] content = File.ReadAllLines(result.Path);
            content.Distinct().Should().HaveCount(recordsCount);
        }

        [Test]
        public async Task CreateDataFileAsync_ShouldReturnSettingsWithWrittenFile_WhenFullNameShouldBeCreatedOnTheFly()
        {
            // Arrange
            const int recordsCount = 100;

            string[] fields = MyFirstProviderXmlGenerator.EntityColumns;
            string identifier = fields.First();

            string xmlData = MyFirstProviderXmlGenerator.GenerateRecords(recordsCount, fields);
            File.WriteAllText(_dataFilePath, xmlData);

            List<IndexedFieldMap> fieldsMapping = PrepareMapping(fields, identifier);

            fieldsMapping.Add(CreateMap("Full Name", false, fieldsMapping.Count, FieldMapType.EntityFullName));

            IntegrationPointInfo integrationPointInfo = _fxt.Build<IntegrationPointInfo>()
                .With(x => x.FieldMap, fieldsMapping)
                .With(x => x.SourceConfiguration, _dataFilePath)
                .Create();

            SetupBatchIdsFile(fieldsMapping);

            CustomProviderBatch batch = _fxt.Build<CustomProviderBatch>()
                .With(x => x.IDsFilePath, _dataIdsFilePath)
                .Create();

            // Act
            DataSourceSettings result = await _sut.CreateDataFileAsync(batch, _sourceProvider, integrationPointInfo, _importDirectory).ConfigureAwait(false);

            // Assert
            string[] content = File.ReadAllLines(result.Path);
            content.Distinct().Should().HaveCount(recordsCount);
            content.All(x => x.Split(result.ColumnDelimiter).Length == fieldsMapping.Count);
        }

        private IEnumerable<string> AsEnumerable(IDataReader dataReader, Func<IDataReader, string> formatter)
        {
            while (dataReader.Read())
            {
                string value = formatter(dataReader);
                yield return value;
            }
        }

        private void SetupStorageService()
        {
            _storageServiceFake = new Mock<IRelativityStorageService>();
            _storageServiceFake.Setup(x => x.ReadAllLinesAsync(It.IsAny<string>()))
                .Returns((string path) => Task.FromResult(File.ReadAllLines(path).ToList()));
            _storageServiceFake.Setup(x => x.CreateFileOrTruncateExistingAsync(It.IsAny<string>()))
                .Returns((string path) =>
                {
                    FileStream fs = File.Create(path);
                    return Task.FromResult<StorageStream>(new FakeStorageStream(fs));
                });
        }

        private List<IndexedFieldMap> PrepareMapping(string[] fields, string identifier)
        {
            return fields.Select((x, idx) => CreateMap(x, x == identifier, idx)).ToList();
        }

        private IndexedFieldMap CreateMap(string name, bool isIdentifier, int index, FieldMapType mapType = FieldMapType.Normal)
        {
            return new IndexedFieldMap(
                new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = name,
                        IsIdentifier = isIdentifier
                    },
                    DestinationField = new FieldEntry
                    {
                        DisplayName = name
                    },
                },
                mapType,
                index);
        }

        private void SetupBatchIdsFile(List<IndexedFieldMap> fieldsMapping)
        {
            FieldEntry identifier = fieldsMapping
                .Single(x => x.FieldMap.SourceField.IsIdentifier)
                .FieldMap.SourceField;

            DataSourceProviderConfiguration configuration = new DataSourceProviderConfiguration
            {
                Configuration = _dataFilePath
            };

            IDataReader idsDataReader = _sourceProvider.GetBatchableIds(identifier, configuration);
            File.WriteAllLines(_dataIdsFilePath, AsEnumerable(idsDataReader, dr => dr.GetString(0)));
        }
    }
}
