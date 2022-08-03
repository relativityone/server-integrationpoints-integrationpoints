using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class SavedSearchRepository : ISavedSearchRepository
    {
        private Guid _runId;
        private int _documentsRetrieved = 0;
        private int _totalDocumentsRetrieved = 0;
        private bool _startedRetrieving = false;

        private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

        private readonly IRelativityObjectManager _objectManager;
        private readonly int _savedSearchId;

        public SavedSearchRepository(
            IRelativityObjectManager objectManager,
            int savedSearchId)
        {
            _objectManager = objectManager;
            _savedSearchId = savedSearchId;
        }

        public async Task<ArtifactDTO[]> RetrieveNextDocumentsAsync()
        {
            if (!_startedRetrieving)
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = Guid.Parse(ObjectTypeGuids.Document)
                    },
                    Fields = new List<FieldRef>
                    {
                        new FieldRef { Guid = Guid.Parse(DocumentFieldGuids.ControlNumber) }
                    },
                    Condition = $"'ArtifactId' IN SAVEDSEARCH {_savedSearchId}"
                };


                ExportInitializationResults export = await _objectManager.InitializeExportAsync(queryRequest, 1).ConfigureAwait(false);
                _runId = export.RunID;
                _totalDocumentsRetrieved = (int)export.RecordCount;
                _startedRetrieving = true;
            }

            RelativityObjectSlim[] resultsBlock = await _objectManager
                .RetrieveResultsBlockFromExportAsync(_runId, _totalDocumentsRetrieved - _documentsRetrieved, _documentsRetrieved)
                .ConfigureAwait(false);

            if (resultsBlock != null && resultsBlock.Any())
            {
                ArtifactDTO[] results = resultsBlock.Select(
                    x => new ArtifactDTO(
                        x.ArtifactID,
                        _DOCUMENT_ARTIFACT_TYPE_ID,
                        (string)x.Values[0],
                        new ArtifactFieldDTO[0])).ToArray();

                _documentsRetrieved += results.Length;

                return results;
            }

            throw new IntegrationPointsException($"Failed to retrieve for saved search ID {_savedSearchId}")
            {
                ExceptionSource = IntegrationPointsExceptionSource.GENERIC
            };
        }

        public bool AllDocumentsRetrieved()
        {
            return _startedRetrieving && _totalDocumentsRetrieved == _documentsRetrieved;
        }
    }
}