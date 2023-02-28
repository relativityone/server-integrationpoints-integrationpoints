using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
    [TestFixture, Category("Unit")]
    public class DocumentTransferDataReaderTests : ExportTransferDataReaderTestsBase
    {
        protected override ExportTransferDataReaderBase CreateDataReaderTestInstance()
        {
            return new DocumentTransferDataReader(
                _exportService,
                _templateFieldEntries,
                _scratchRepositories,
                _relativityObjectManager,
                _documentRepository,
                Substitute.For<IAPILog>(),
                Substitute.For<IQueryFieldLookupRepository>(),
                Substitute.For<IFileRepository>(),
                false,
                _SOURCE_WORKSPACE_ARTIFACTID);
        }

        protected override ExportTransferDataReaderBase CreateDataReaderTestInstanceWithParameters(
            IExporterService relativityExportService,
            FieldMap[] fieldMappings,
            IScratchTableRepository[] scratchTableRepositories)
        {
            return new DocumentTransferDataReader(
                relativityExportService,
                fieldMappings,
                scratchTableRepositories,
                _relativityObjectManager,
                _documentRepository,
                Substitute.For<IAPILog>(),
                Substitute.For<IQueryFieldLookupRepository>(),
                Substitute.For<IFileRepository>(),
                false,
                _SOURCE_WORKSPACE_ARTIFACTID);
        }
    }
}
