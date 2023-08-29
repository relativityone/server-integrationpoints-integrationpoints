using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.Exporter.Validators;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Base
{
    public abstract class ExporterServiceBase : IExporterService
    {
        protected readonly IQueryFieldLookupRepository QueryFieldLookupRepository;
        protected readonly SourceConfiguration SourceConfiguration;
        protected readonly ExportInitializationResultsDto ExportJobInfo;
        protected readonly FieldMap[] MappedFields;
        protected readonly HashSet<int> LongTextFieldArtifactIds;
        protected readonly HashSet<int> MultipleObjectFieldArtifactIds;
        protected readonly HashSet<int> SingleChoiceFieldsArtifactIds;
        protected readonly IAPILog Logger;
        protected readonly IDocumentRepository DocumentRepository;
        protected readonly IRelativityObjectManager RelativityObjectManager;
        protected readonly IJobStopManager JobStopManager;
        protected readonly IFileRepository FileRepository;
        protected readonly ISerializer Serializer;
        protected readonly int[] FieldArtifactIds;
        protected readonly FieldEntry IdentifierField;

        protected IDataTransferContext Context { get; set; }

        protected int RetrievedDataCount { get; set; }

        internal ExporterServiceBase(
            IDocumentRepository documentRepository,
            IRelativityObjectManager relativityObjectManager,
            IRepositoryFactory repositoryFactory,
            IJobStopManager jobStopManager,
            IHelper helper,
            IFileRepository fileRepository,
            ISerializer serializer,
            FieldMap[] mappedFields,
            int startAt,
            SourceConfiguration sourceConfiguration,
            int searchArtifactId)
            : this(mappedFields, jobStopManager, helper)
        {
            DocumentRepository = documentRepository;
            FileRepository = fileRepository;
            Serializer = serializer;
            RelativityObjectManager = relativityObjectManager;
            SourceConfiguration = sourceConfiguration;

            ValidateDestinationFields(repositoryFactory, mappedFields);

            QueryFieldLookupRepository = repositoryFactory.GetQueryFieldLookupRepository(SourceConfiguration.SourceWorkspaceArtifactId);

            ValidateSourceFields(repositoryFactory, mappedFields);

            try
            {
                ExportJobInfo = SourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch
                    ? DocumentRepository.InitializeSearchExportAsync(searchArtifactId, FieldArtifactIds, startAt).GetAwaiter().GetResult()
                    : DocumentRepository.InitializeProductionExportAsync(searchArtifactId, FieldArtifactIds, startAt).GetAwaiter().GetResult();
                Logger.LogInformation("Retrieved ExportJobInfo in ExporterServiceBase. Run: {runId}, rows: {rowCount}", ExportJobInfo?.RunID, ExportJobInfo?.RecordCount);
            }
            catch (Exception exception)
            {
                LogCreatingExporterServiceError(exception);
                // NOTE: If we get an exception, we cannot be exactly sure what the real error is,
                // however, it is more than likely that you do not have Export or Saved Search permissions.
                throw new IntegrationPointsException(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_EXPORT, exception);
            }
        }

        protected ExporterServiceBase(FieldMap[] mappedFields, IJobStopManager jobStopManager, IHelper helper)
        {
            SingleChoiceFieldsArtifactIds = new HashSet<int>();
            MultipleObjectFieldArtifactIds = new HashSet<int>();
            LongTextFieldArtifactIds = new HashSet<int>();
            RetrievedDataCount = 0;
            MappedFields = mappedFields;
            JobStopManager = jobStopManager;
            Logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityExporterService>();
            FieldArtifactIds = mappedFields.Select(field => int.Parse(field.SourceField.FieldIdentifier)).ToArray();
            IdentifierField = mappedFields.First(x => x.SourceField.IsIdentifier).SourceField;
        }

        public virtual bool HasDataToRetrieve => (TotalRecordsFound > RetrievedDataCount) && !JobStopManager.IsStopRequested();

        public virtual int TotalRecordsFound => (int) ExportJobInfo.RecordCount;

        public abstract IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration);

        public abstract ArtifactDTO[] RetrieveData(int size);

        public virtual Task LogFileSharesSummaryAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (Context?.DataReader != null)
            {
                Context.DataReader.Dispose();
                Context.DataReader = null;
                Context = null;
            }
        }

        private void ValidateSourceFields(IRepositoryFactory sourceRepositoryFactory, IEnumerable<FieldMap> mappedFields)
        {
            var nestedAndMultiValuesValidator = new NestedAndMultiValuesFieldValidator(Logger);

            foreach (FieldEntry source in mappedFields.Select(f => f.SourceField))
            {
                int artifactID = Convert.ToInt32(source.FieldIdentifier);
                ViewFieldInfo fieldInfo = QueryFieldLookupRepository.GetFieldByArtifactID(artifactID);

                switch (fieldInfo.FieldType)
                {
                    case FieldTypeHelper.FieldType.Objects:
                        IFieldQueryRepository fieldQueryRepository = sourceRepositoryFactory.GetFieldQueryRepository(SourceConfiguration.SourceWorkspaceArtifactId);
                        ArtifactDTO identifierField = fieldQueryRepository.RetrieveIdentifierField(fieldInfo.AssociativeArtifactTypeID);
                        string identifierFieldName = (string) identifierField.Fields.First(field => field.Name == "Name").Value;
                        IObjectRepository objectRepository =
                            sourceRepositoryFactory.GetObjectRepository(
                                SourceConfiguration.SourceWorkspaceArtifactId,
                                fieldInfo.AssociativeArtifactTypeID);
                        ArtifactDTO[] objects = objectRepository.GetFieldsFromObjects(new[] {identifierFieldName})
                            .GetAwaiter().GetResult();
                        nestedAndMultiValuesValidator.VerifyObjectField(fieldInfo.FieldArtifactId, fieldInfo.DisplayName, objects);
                        break;

                    case FieldTypeHelper.FieldType.MultiCode:
                        ICodeRepository codeRepository =
                            sourceRepositoryFactory.GetCodeRepository(SourceConfiguration.SourceWorkspaceArtifactId);
                        ArtifactDTO[] codes = codeRepository.RetrieveCodeAsync(fieldInfo.DisplayName).GetAwaiter().GetResult();
                        nestedAndMultiValuesValidator.VerifyChoiceField(fieldInfo.FieldArtifactId, fieldInfo.DisplayName, codes);
                        break;
                }
            }
        }

        private void ValidateDestinationFields(IRepositoryFactory targetRepositoryFactory, FieldMap[] mappedFields)
        {
            IFieldQueryRepository targetFieldQueryRepository = targetRepositoryFactory.GetFieldQueryRepository(SourceConfiguration.TargetWorkspaceArtifactId);
            var destinationFieldsValidator = new DestinationFieldsValidator(targetFieldQueryRepository, Logger);
            destinationFieldsValidator.ValidateDestinationFields(mappedFields);
        }

        #region Logging

        protected void LogCreatingExporterServiceError(Exception exception)
        {
            Logger.LogError(exception, "Failed to create RelativityExporterService.");
        }

        protected virtual void LogRetrievingDataError(Exception ex)
        {
            Logger.LogError(ex, "Error occurred during data retrieval.");
        }

        #endregion
    }
}
