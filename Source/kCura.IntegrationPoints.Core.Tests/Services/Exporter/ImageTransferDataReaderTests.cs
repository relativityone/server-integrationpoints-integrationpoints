using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Core.Services.Exporter.Images;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
    [TestFixture, Category("Unit")]
    public class ImageTransferDataReaderTests : ExportTransferDataReaderTestsBase
    {
        protected override ExportTransferDataReaderBase CreateDataReaderTestInstance()
        {
            return new ImageTransferDataReader(
                _exportService,
                _templateFieldEntries,
                NSubstitute.Substitute.For<IAPILog>(),
                _scratchRepositories);
        }

        protected override ExportTransferDataReaderBase CreateDataReaderTestInstanceWithParameters(
            IExporterService relativityExportService,
            FieldMap[] fieldMappings,
            IScratchTableRepository[] scratchTableRepositories)
        {
            return new ImageTransferDataReader(
                relativityExportService,
                fieldMappings,
                NSubstitute.Substitute.For<IAPILog>(),
                scratchTableRepositories);
        }
    }
}
