using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Base
{
    public abstract class ExportTransferDataReaderBase : RelativityReaderBase
    {
        private readonly IAPILog _logger;

        protected const string _FIELD_IDENTIFIER_PARSE_ERROR_MSG =
            "Parsing field identifier ({@fieldId}) FAILED.";

        protected readonly IExporterService RelativityExporterService;
        protected readonly int FolderPathFieldSourceArtifactId;
        protected readonly IScratchTableRepository[] ScratchTableRepositories;
        public const int FETCH_ARTIFACTDTOS_BATCH_SIZE = 200;

        protected ExportTransferDataReaderBase(
            IExporterService relativityExportService,
            FieldMap[] fieldMappings,
            IScratchTableRepository[] scratchTableRepositories,
            IAPILog logger,
            bool useDynamicFolderPath) :
                base(GenerateDataColumnsFromFieldEntries(fieldMappings, useDynamicFolderPath))
        {
            _logger = logger.ForContext<ExportTransferDataReaderBase>();
            ScratchTableRepositories = scratchTableRepositories;
            RelativityExporterService = relativityExportService;

            FieldMap folderPathInformationField = GetFolderPathInformationField(fieldMappings);
            if (folderPathInformationField != null)
            {
                bool result = int.TryParse(folderPathInformationField.SourceField.FieldIdentifier, out FolderPathFieldSourceArtifactId);
                if (!result)
                {
                    throw LogFieldIdentifierParseError(folderPathInformationField.SourceField.FieldIdentifier);
                }
            }
        }

        protected override ArtifactDTO[] FetchArtifactDTOs()
        {
            ArtifactDTO[] artifacts = RelativityExporterService.RetrieveData(FETCH_ARTIFACTDTOS_BATCH_SIZE);
            List<int> artifactIds = artifacts.Select(x => x.ArtifactId).ToList();

            foreach (IScratchTableRepository repository in ScratchTableRepositories)
            {
                repository.AddArtifactIdsIntoTempTable(artifactIds);
            }

            return artifacts;
        }

        protected override bool AllArtifactsFetched()
        {
            return !RelativityExporterService.HasDataToRetrieve;
        }

        protected static DataColumn[] GenerateDataColumnsFromFieldEntries(FieldMap[] mappingFields, bool useDynamicFolderPath)
        {
            List<FieldEntry> sourceFields = mappingFields.Select(field => field.SourceField).ToList();

            InsertSpecialSourceFields(sourceFields);
            InsertFolderFields(sourceFields, mappingFields, useDynamicFolderPath);
            InsertFileTypeField(sourceFields);

            return sourceFields.Select(x => new DataColumn(x.FieldIdentifier)).ToArray();
        }

        private static void InsertFileTypeField(List<FieldEntry> sourceFields)
        {
            sourceFields.Add(new FieldEntry()
            {
                DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FILE_TYPE_FIELD_NAME,
                FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FILE_TYPE_FIELD,
                FieldType = FieldType.String
            });

            sourceFields.Add(new FieldEntry()
            {
                DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD_NAME,
                FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD,
                FieldType = FieldType.String
            });
        }

        public override string GetDataTypeName(int i)
        {
            return GetFieldType(i).ToString();
        }

        public override Type GetFieldType(int i)
        {
            string fieldIdentifier = GetName(i);
            int fieldArtifactId;
            bool isFieldIdentifierNumeric = int.TryParse(fieldIdentifier, out fieldArtifactId);
            object value = null;
            if (isFieldIdentifierNumeric)
            {
                value = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId).Value;
            }
            return value?.GetType() ?? typeof(object);
        }

        private static FieldMap GetFolderPathInformationField(FieldMap[] fieldMappings)
        {
            return fieldMappings.FirstOrDefault(mappedField => mappedField.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
        }

        private static void InsertSpecialSourceFields(List<FieldEntry> sourceFields)
        {
            // we will always import this native file location
            sourceFields.Add(new FieldEntry
            {
                DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
                FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD,
                FieldType = FieldType.String
            });
            sourceFields.Add(new FieldEntry
            {
                DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME,
                FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD,
                FieldType = FieldType.String
            });
            sourceFields.Add(new FieldEntry
            {
                DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD_NAME,
                FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD,
                FieldType = FieldType.String
            });
            sourceFields.Add(new FieldEntry
            {
                DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD_NAME,
                FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD,
                FieldType = FieldType.String
            });
        }

        private static void InsertFolderFields(List<FieldEntry> sourceFields, FieldMap[] mappingFields, bool useDynamicFolderPath)
        {
            FieldMap folderPathInformationField = GetFolderPathInformationField(mappingFields);
            if (folderPathInformationField != null)
            {
                InsertFolderPathInformationField(sourceFields, folderPathInformationField);
            }
            else if (useDynamicFolderPath)
            {
                InsertDynamicFolderPathField(sourceFields);
            }
        }

        private static void InsertFolderPathInformationField(List<FieldEntry> sourceFields, FieldMap folderPathInformationField)
        {
            if (folderPathInformationField.DestinationField.FieldIdentifier == null)
            {
                sourceFields.Remove(folderPathInformationField.SourceField);
            }

            sourceFields.Add(new FieldEntry()
            {
                DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD_NAME,
                FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD,
                FieldType = FieldType.String
            });
        }

        private static void InsertDynamicFolderPathField(List<FieldEntry> sourceFields)
        {
            sourceFields.Add(new FieldEntry()
            {
                DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME,
                FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD,
                FieldType = FieldType.String
            });
        }

        private IntegrationPointsException LogFieldIdentifierParseError(string fieldIdentifier)
        {
            _logger.LogError(_FIELD_IDENTIFIER_PARSE_ERROR_MSG, fieldIdentifier);
            return new IntegrationPointsException($"Parsing field identifier ({fieldIdentifier}) FAILED.");
        }

    }
}
