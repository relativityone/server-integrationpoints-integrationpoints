using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;
using Relativity.DataExchange.Service;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using FieldType = Relativity.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
    [TestFixture, Category("Unit")]
    class ExportNativeSettingsValidatorTests
    {
        private const int _RDO_ARTIFACT_TYPE_ID = 1;
        private readonly ISerializer _serializer = Substitute.For<ISerializer>();
        private readonly IExportSettingsBuilder _settingsBuilder = Substitute.For<IExportSettingsBuilder>();
        private readonly IExportFileBuilder _fileBuilder = Substitute.For<IExportFileBuilder>();
        private readonly IExportFieldsService _exportFieldsService = Substitute.For<IExportFieldsService>();
        private ExportNativeSettingsValidator _subjectUnderTests;

        [SetUp]
        public void SetUp()
        {
            _serializer.Deserialize<ExportUsingSavedSearchSettings>(Arg.Any<string>())
                .Returns(new ExportUsingSavedSearchSettings());

            _serializer.Deserialize<ImportSettings>(Arg.Any<string>())
                .Returns(new ImportSettings());

            _serializer.Deserialize<IEnumerable<FieldMap>>(Arg.Any<string>())
                .Returns(new List<FieldMap>());

            _exportFieldsService.GetAllExportableFields(Arg.Any<int>(), Arg.Any<int>()).Returns(
                new FieldEntry[]
                {
                    new FieldEntry()
                    {
                        FieldType = FieldType.String
                    }
                }
            );

            _subjectUnderTests = new ExportNativeSettingsValidator(_serializer, _settingsBuilder, _fileBuilder, _exportFieldsService);
        }

        [TestCase(ExportFile.ExportType.AncestorSearch, _RDO_ARTIFACT_TYPE_ID, false)]
        [TestCase(ExportFile.ExportType.AncestorSearch, ArtifactType.Document, true)]
        [TestCase(ExportFile.ExportType.ArtifactSearch, _RDO_ARTIFACT_TYPE_ID, true)]
        [TestCase(ExportFile.ExportType.ArtifactSearch, ArtifactType.Document, true)]
        [TestCase(ExportFile.ExportType.ParentSearch, _RDO_ARTIFACT_TYPE_ID, true)]
        [TestCase(ExportFile.ExportType.ParentSearch, ArtifactType.Document, true)]
        [TestCase(ExportFile.ExportType.Production, ArtifactType.Document, true)]
        [TestCase(ExportFile.ExportType.Production, _RDO_ARTIFACT_TYPE_ID, true)]
        public void ValidateFileTypeRdoExportSettings(ExportFile.ExportType exportType, int artifactTypeId, bool expectedResult)
        {
            ExtendedExportFile exportFile = new ExtendedExportFile(artifactTypeId)
            {
                TypeOfExport = exportType,
                ExportNative = true,
                CaseInfo = new CaseInfo
                {
                    ArtifactID    = 123456
                }
            };

            var model = new IntegrationPointProviderValidationModel()
            {
                DestinationConfiguration = string.Empty,
                SourceConfiguration = string.Empty,
                FieldsMap = new List<FieldMap>(),
            };

            _fileBuilder.Create(Arg.Any<ExportSettings>()).Returns(exportFile);
            ValidationResult result = _subjectUnderTests.Validate(model);

            Assert.That(result.IsValid, Is.EqualTo(expectedResult));
        }
    }
}
