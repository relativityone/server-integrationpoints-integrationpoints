using System;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tagging
{
    public class Tagger : ITagger
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDataSynchronizer _synchronizer;
        private readonly IDiagnosticLog _diagnosticLog;
        private readonly IAPILog _logger;
        private readonly FieldMap[] _fields;
        private readonly string _importConfig;

        public Tagger(
            IDocumentRepository documentRepository,
            IDataSynchronizer synchronizer,
            IHelper helper,
            FieldMap[] fields,
            string importConfig,
            IDiagnosticLog diagnosticLog)
        {
            _documentRepository = documentRepository;
            _synchronizer = synchronizer;
            _fields = fields;
            _importConfig = importConfig;
            _diagnosticLog = diagnosticLog;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<Tagger>();
        }

        public void TagDocuments(TagsContainer tagsContainer, IScratchTableRepository scratchTableRepository)
        {
            try
            {
                if (scratchTableRepository.GetCount() < 1)
                {
                    return;
                }

                FieldMap identifierField = GetIdentifierField();
                LogStartTaggingDocuments(identifierField);
                DataColumn[] columns =
                {
                    new DataColumn(identifierField.SourceField.FieldIdentifier),
                    new DataColumnWithValue(Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD, tagsContainer.SourceWorkspaceDto.Name),
                    new DataColumnWithValue(Domain.Constants.SPECIAL_SOURCEJOB_FIELD, tagsContainer.SourceJobDto.Name)
                };

                int identifierFieldId = Convert.ToInt32(identifierField.SourceField.FieldIdentifier);
                using (var reader = new TempTableReader(_documentRepository, scratchTableRepository, columns, identifierFieldId))
                {
                    FieldMap[] fieldsToPush = { identifierField };
                    var documentTransferContext = new DefaultTransferContext(reader);
                    _synchronizer.SyncData(documentTransferContext, fieldsToPush, _importConfig, null, _diagnosticLog);
                }
            }
            catch (Exception e)
            {
                throw LogAndWrapException(e);
            }
        }

        private void LogStartTaggingDocuments(FieldMap identifierField)
        {
            _logger.LogInformation("Start tagging documents. Identifier field ID: {identifierFieldID}", identifierField.SourceField.FieldIdentifier);
        }

        private FieldMap GetIdentifierField()
        {
            return _fields.First(f => f.FieldMapType == FieldMapTypeEnum.Identifier);
        }

        private IntegrationPointsException LogAndWrapException(Exception e)
        {
            string errorMessage = $"Error occurred during document tagging in {nameof(Tagger)}";
            _logger.LogError(e, errorMessage);
            throw new IntegrationPointsException(errorMessage, e);
        }
    }
}