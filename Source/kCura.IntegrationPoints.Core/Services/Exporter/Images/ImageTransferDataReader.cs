using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Images
{
    public class ImageTransferDataReader : ExportTransferDataReaderBase
    {
        private readonly IAPILog _logger;

        public ImageTransferDataReader(
            IExporterService relativityExportService,
            FieldMap[] fieldMappings,
            IAPILog logger,
            IScratchTableRepository[] scratchTableRepositories) :
            base(relativityExportService, fieldMappings, scratchTableRepositories, logger, false)
        {
            _logger = logger.ForContext<ImageTransferDataReader>();
        }

        protected override ArtifactDTO[] FetchArtifactDTOs()
        {
            try
            {
                ArtifactDTO[] artifacts = RelativityExporterService.RetrieveData(FETCH_ARTIFACTDTOS_BATCH_SIZE);

                List<int> artifactIds = artifacts.Select(x => x.ArtifactId).Distinct().ToList();
                foreach (IScratchTableRepository repository in ScratchTableRepositories)
                {
                    repository.AddArtifactIdsIntoTempTable(artifactIds);
                }

                return artifacts;
            }
            catch (Exception e)
            {
                throw LogFetchArtifactDTOsError(e);
            }
        }

        public override object GetValue(int i)
        {
            string fieldIdentifier = "";
            int fieldArtifactId = 0;
            ArtifactFieldDTO retrievedField = null;
            try
            {
                fieldIdentifier = GetName(i);

                bool isFieldIdentifierNumericValue = int.TryParse(fieldIdentifier, out fieldArtifactId);
                if (isFieldIdentifierNumericValue)
                {
                    retrievedField = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId);
                    return retrievedField.Value;
                }

                switch (fieldIdentifier)
                {
                    case IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD:
                        retrievedField = CurrentArtifact.GetFieldForIdentifier(FolderPathFieldSourceArtifactId);
                        return retrievedField.Value;
                    case IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD:
                        ArtifactFieldDTO fileLocationField = CurrentArtifact.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME);
                        return fileLocationField.Value;
                    case IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD:
                        ArtifactFieldDTO nameField = CurrentArtifact.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME);
                        return nameField.Value;
                    case IntegrationPoints.Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD:
                        ArtifactFieldDTO imageFilenameField = CurrentArtifact.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD_NAME);
                        return imageFilenameField.Value;
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                throw LogGetValueError(e, i, fieldIdentifier, fieldArtifactId, retrievedField);
            }
        }

        public override bool Read()
        {
            if (ReadEntriesCount % 500 == 0)
            {
                _logger.LogInformation("Read {ReadEntriesCount} documents.", ReadEntriesCount);
            }

            return base.Read();
        }

        private IntegrationPointsException LogGetValueError(Exception e,
            int i,
            string fieldIdentifier,
            int fieldArtifactId,
            ArtifactFieldDTO retrievedField
            )
        {
            var message = $"Error ocurred when getting value for index {i}, fieldIdentifier: {fieldIdentifier}, fieldArtifactId: {fieldArtifactId}, retrievedField: {retrievedField.ArtifactId}";
            var template = "Error ocurred when getting value for index {i}, fieldIdentifier: {fieldIdentifier}, fieldArtifactId: {fieldArtifactId}, retrievedField: {retrievedFieldArtifactId}";
            var exc = new IntegrationPointsException(message, e);
            _logger.LogError(exc, template, i, fieldIdentifier, fieldArtifactId, retrievedField.ArtifactId);
            return exc;
        }

        private IntegrationPointsException LogFetchArtifactDTOsError(Exception e)
        {
            var message = "Error ocurred when fetching ArtifactDTOs";
            var exc = new IntegrationPointsException(message, e);
            _logger.LogError(exc, message);
            return exc;
        }

    }
}
