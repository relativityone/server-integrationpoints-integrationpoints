using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.AdlsHelpers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Images;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
    public class ExporterFactory : IExporterFactory
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IFolderPathReaderFactory _folderPathReaderFactory;
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IFileRepository _fileRepository;
        private readonly IAdlsHelper _adlsHelper;
        private readonly ISerializer _serializer;
        private readonly ILogger<ExporterFactory> _logger;

        public ExporterFactory(
            IRepositoryFactory repositoryFactory,
            IFolderPathReaderFactory folderPathReaderFactory,
            IRelativityObjectManager relativityObjectManager,
            IFileRepository fileRepository,
            ISerializer serializer,
            IAdlsHelper adlsHelper,
            ILogger<ExporterFactory> logger)
        {
            _repositoryFactory = repositoryFactory;
            _folderPathReaderFactory = folderPathReaderFactory;
            _relativityObjectManager = relativityObjectManager;
            _fileRepository = fileRepository;
            _serializer = serializer;
            _adlsHelper = adlsHelper;
            _logger = logger;
        }

        public IExporterService BuildExporter(
            IJobStopManager jobStopManager,
            FieldMap[] mappedFields,
            string serializedSourceConfiguration,
            int savedSearchArtifactID,
            DestinationConfiguration destinationConfiguration,
            IDocumentRepository documentRepository,
            IExportDataSanitizer exportDataSanitizer)
        {
            LogBuildExporterExecutionWithParameters(mappedFields, serializedSourceConfiguration, savedSearchArtifactID, destinationConfiguration);

            SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(serializedSourceConfiguration);

            IExporterService exporter = destinationConfiguration.ImageImport ?
                CreateImageExporterService(
                    jobStopManager,
                    mappedFields,
                    savedSearchArtifactID,
                    destinationConfiguration,
                    sourceConfiguration,
                    documentRepository) :
                CreateRelativityExporterService(
                    jobStopManager,
                    mappedFields,
                    serializedSourceConfiguration,
                    savedSearchArtifactID,
                    destinationConfiguration,
                    documentRepository,
                    exportDataSanitizer);
            return exporter;
        }

        private IExporterService CreateRelativityExporterService(
            IJobStopManager jobStopManager,
            FieldMap[] mappedFields,
            string serializedSourceConfiguration,
            int savedSearchArtifactID,
            DestinationConfiguration destinationConfiguration,
            IDocumentRepository documentRepository,
            IExportDataSanitizer exportDataSanitizer)
        {
            SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(serializedSourceConfiguration);
            int workspaceArtifactID = sourceConfiguration.SourceWorkspaceArtifactId;
            bool useDynamicFolderPath = destinationConfiguration.UseDynamicFolderPath;
            IFolderPathReader folderPathReader = _folderPathReaderFactory.Create(workspaceArtifactID, useDynamicFolderPath);
            const int startAtRecord = 0;

            return new RelativityExporterService(
                documentRepository,
                _relativityObjectManager,
                _repositoryFactory,
                jobStopManager,
                folderPathReader,
                _fileRepository,
                _serializer,
                exportDataSanitizer,
                mappedFields,
                startAtRecord,
                sourceConfiguration,
                savedSearchArtifactID,
                _logger.ForContext<RelativityExporterService>());
        }

        private IExporterService CreateImageExporterService(
            IJobStopManager jobStopManager,
            FieldMap[] mappedFiles,
            int savedSearchArtifactId,
            DestinationConfiguration destinationConfiguration,
            SourceConfiguration sourceConfiguration,
            IDocumentRepository documentRepository)
        {
            int searchArtifactId;
            if (sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch)
            {
                searchArtifactId = savedSearchArtifactId;
            }
            else
            {
                searchArtifactId = sourceConfiguration.SourceProductionId;
            }

            const int startAtRecord = 0;

            return new ImageExporterService(
                documentRepository,
                _relativityObjectManager,
                _repositoryFactory,
                _fileRepository,
                jobStopManager,
                _adlsHelper,
                destinationConfiguration,
                _serializer,
                mappedFiles,
                startAtRecord,
                sourceConfiguration,
                searchArtifactId,
                _logger.ForContext<ImageExporterService>());
        }

        private void LogBuildExporterExecutionWithParameters(
            FieldMap[] mappedFields,
            string config,
            int savedSearchArtifactId,
            DestinationConfiguration destinationConfiguration)
        {
            IEnumerable<FieldMap> mappedFieldsWithoutFieldNames = mappedFields.Select(mf => new FieldMap
            {
                SourceField = CreateFieldEntryWithoutName(mf.SourceField),
                DestinationField = CreateFieldEntryWithoutName(mf.DestinationField),
                FieldMapType = mf.FieldMapType
            });

            var msgBuilder = new StringBuilder("Building Exporter with parameters: \n");
            msgBuilder.AppendLine("mappedFields {@mappedFields} ");
            msgBuilder.AppendLine("config {config} ");
            msgBuilder.AppendLine("savedSearchArtifactId {savedSearchArtifactId} ");
            msgBuilder.AppendLine("userImportApiSettings {userImportApiSettings}");
            string msgTemplate = msgBuilder.ToString();
            _logger.LogInformation(
                msgTemplate,
                mappedFieldsWithoutFieldNames,
                config,
                savedSearchArtifactId,
                destinationConfiguration);
        }

        private FieldEntry CreateFieldEntryWithoutName(FieldEntry entry)
        {
            var newEntry = new FieldEntry
            {
                FieldIdentifier = entry.FieldIdentifier,
                FieldType = entry.FieldType,
                DisplayName = Domain.Constants.SENSITIVE_DATA_REMOVED_FOR_LOGGING
            };
            return newEntry;
        }
    }
}
