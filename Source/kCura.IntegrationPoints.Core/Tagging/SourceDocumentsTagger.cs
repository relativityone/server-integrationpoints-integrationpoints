using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tagging
{
    internal class SourceDocumentsTagger : ISourceDocumentsTagger
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMassUpdateHelper _massUpdateHelper;
        private readonly IAPILog _logger;

        public SourceDocumentsTagger(
            IDocumentRepository documentRepository,
            IAPILog logger,
            IMassUpdateHelper massUpdateHelper)
        {
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            _massUpdateHelper = massUpdateHelper ?? throw new ArgumentNullException(nameof(massUpdateHelper));
            _logger = (logger ?? throw new ArgumentNullException(nameof(logger)))
                .ForContext<SourceDocumentsTagger>();
        }

        public async Task TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
            IScratchTableRepository documentsToTagRepository,
            int destinationWorkspaceInstanceID,
            int jobHistoryInstanceID)
        {
            if (documentsToTagRepository == null)
            {
                throw new ArgumentNullException(nameof(documentsToTagRepository));
            }

            _logger.LogInformation("Tagging documents in source workspace started.");
            FieldUpdateRequestDto[] documentTagsInSourceWorkspace = GetTagsValues(
                destinationWorkspaceInstanceID,
                jobHistoryInstanceID);

            await _massUpdateHelper
                .UpdateArtifactsAsync(
                    documentsToTagRepository,
                    documentTagsInSourceWorkspace,
                    _documentRepository)
                .ConfigureAwait(false);

            _logger.LogInformation("Tagging documents in source workspace completed.");
        }

        private static FieldUpdateRequestDto[] GetTagsValues(int destinationWorkspaceInstanceID, int jobHistoryInstanceID)
        {
            FieldUpdateRequestDto[] documentTagsInSourceWorkspace =
                        {
                new FieldUpdateRequestDto(
                    DocumentFieldGuids.RelativityDestinationCaseGuid,
                    new MultiObjectReferenceDto(destinationWorkspaceInstanceID)),
                new FieldUpdateRequestDto(
                    DocumentFieldGuids.JobHistoryGuid,
                    new MultiObjectReferenceDto(jobHistoryInstanceID)),
            };
            return documentTagsInSourceWorkspace;
        }
    }
}
