using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public class RelativityExporterService : ExporterServiceBase
    {
        private readonly IFolderPathReader _folderPathReader;
        private readonly IExportDataSanitizer _exportDataSanitizer;

        public RelativityExporterService(
            IDocumentRepository documentRepository, 
            IRelativityObjectManager relativityObjectManager,
            IRepositoryFactory repositoryFactory,
            IJobStopManager jobStopManager, 
            IHelper helper,
            IFolderPathReader folderPathReader,
            IFileRepository fileRepository,
            ISerializer serializer,
            IExportDataSanitizer exportDataSanitizer,
            FieldMap[] mappedFields, 
            int startAt, 
            SourceConfiguration sourceConfiguration, 
            int searchArtifactId)
            : base(
                documentRepository,
                relativityObjectManager,
                repositoryFactory,
                jobStopManager, 
                helper,
                fileRepository,
                serializer,
                mappedFields, 
                startAt, 
                sourceConfiguration, 
                searchArtifactId)
        {
            _folderPathReader = folderPathReader;
            _exportDataSanitizer = exportDataSanitizer;
        }

        public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
        {
            var documentTransferDataReader = new DocumentTransferDataReader(
                this, 
                MappedFields,
                transferConfiguration.ScratchRepositories, 
                RelativityObjectManager,
                DocumentRepository,
                Logger,
                QueryFieldLookupRepository,
                FileRepository,
                transferConfiguration.ImportSettings.UseDynamicFolderPath,
                SourceConfiguration.SourceWorkspaceArtifactId);
            var exporterTransferContext = 
                new ExporterTransferContext(documentTransferDataReader, transferConfiguration)
                    { TotalItemsFound = TotalRecordsFound };
            return Context ?? (Context = exporterTransferContext);
        }

        public override ArtifactDTO[] RetrieveData(int size)
        {
            Logger.LogInformation("Start retrieving data in RelativityExporterService. Size: {size}, export type: {typeOfExport}, FieldArtifactIds size: {avfIdsSize}",
                size, SourceConfiguration?.TypeOfExport, FieldArtifactIds.Length);

            IList<RelativityObjectSlimDto> retrievedData = DocumentRepository
                    .RetrieveResultsBlockFromExportAsync(ExportJobInfo, size, RetrievedDataCount)
                    .GetAwaiter().GetResult();

            Logger.LogInformation($"Retrieved {retrievedData.Count} documents in ImageExporterService");

            var result = new List<ArtifactDTO>(size);

            foreach (RelativityObjectSlimDto data in retrievedData)
            {
                var fields = new List<ArtifactFieldDTO>(FieldArtifactIds.Length);

                int documentArtifactID = data.ArtifactID;

                string itemIdentifier = data.FieldValues[IdentifierField.ActualName].ToString();
                SetupBaseFieldsAsync(data.FieldValues.Values.ToArray(), fields, itemIdentifier).GetAwaiter().GetResult();

                // TODO: replace String.empty
                string textIdentifier = string.Empty;
                result.Add(new ArtifactDTO(documentArtifactID, (int) ArtifactType.Document, textIdentifier, fields));
            }

            Logger.LogInformation("Before setting folder paths for documents");
            _folderPathReader.SetFolderPaths(result);
            Logger.LogInformation("After setting folder paths for documents");
            RetrievedDataCount += result.Count;
            return result.ToArray();
        }

        private async Task SetupBaseFieldsAsync(object[] fieldsValue, List<ArtifactFieldDTO> fields, string itemIdentifier)
        {
            for (int index = 0; index < FieldArtifactIds.Length; index++)
            {
                string fieldName = ExportJobInfo.FieldNames[index];
                int artifactID = FieldArtifactIds[index];
                FieldTypeHelper.FieldType fieldType = QueryFieldLookupRepository.GetFieldTypeByArtifactID(artifactID);
                object initialValue = fieldsValue[index];

                object value = await SanitizeFieldIfNeededAsync(fieldName, fieldType, initialValue, itemIdentifier)
                    .ConfigureAwait(false);

                fields.Add(new ArtifactFieldDTO
                {
                    Name = fieldName,
                    ArtifactId = artifactID,
                    Value = value?.ToString(),
                    FieldType = fieldType
                });
            }
        }

        private Task<object> SanitizeFieldIfNeededAsync(string fieldName, FieldTypeHelper.FieldType fieldType, object initialValue, string itemIdentifier)
        {
            int sourceWorkspaceArtifactID = SourceConfiguration.SourceWorkspaceArtifactId;

            if (_exportDataSanitizer.ShouldSanitize(fieldType))
            {
                return _exportDataSanitizer
                    .SanitizeAsync(
                        sourceWorkspaceArtifactID,
                        IdentifierField.ActualName,
                        itemIdentifier,
                        fieldName,
                        fieldType,
                        initialValue);
            }

            return Task.FromResult(initialValue);
        }
    }
}