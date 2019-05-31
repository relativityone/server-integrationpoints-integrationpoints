using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tagging
{
	internal class SourceDocumentsTagger : ISourceDocumentsTagger
	{
		private readonly IDocumentRepository _documentRepository;
		private readonly IConfig _config;
		private readonly IAPILog _logger;

		public SourceDocumentsTagger(IDocumentRepository documentRepository, IConfig config, IAPILog logger)
		{
			_documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
			_config = config ?? throw new ArgumentNullException(nameof(documentRepository));
			_logger = logger?.ForContext<SourceDocumentsTagger>();
		}

		public async Task TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
			IScratchTableRepository documentsToTagRepository,
			int destinationWorkspaceInstanceId,
			int jobHistoryInstanceId)
		{
			if (documentsToTagRepository == null)
			{
				throw new ArgumentNullException(nameof(documentsToTagRepository));
			}

			int numberOfDocuments = documentsToTagRepository.GetCount();
			if (numberOfDocuments <= 0)
			{
				LogNoDocumentsToTag();
				return;
			}

			int batchSize = ReadBatchSizeFromConfigAndValidateValue();

			LogTaggingStarted(numberOfDocuments, batchSize);
			FieldUpdateRequestDto[] documentTagsInSourceWorkspace = GetTagsValues(
				destinationWorkspaceInstanceId,
				jobHistoryInstanceId);

			for (int processedCount = 0; processedCount < numberOfDocuments; processedCount += batchSize)
			{
				await
					TagBatchOfDocumentsAsync(
						documentsToTagRepository,
						batchSize,
						processedCount,
						documentTagsInSourceWorkspace)
					.ConfigureAwait(false);
			}
		}

		private int ReadBatchSizeFromConfigAndValidateValue()
		{
			int batchSize = _config.SourceWorkspaceTaggerBatchSize;
			ValidateBatchSize(batchSize);
			return batchSize;
		}

		private static FieldUpdateRequestDto[] GetTagsValues(int destinationWorkspaceInstanceId, int jobHistoryInstanceId)
		{
			FieldUpdateRequestDto[] documentTagsInSourceWorkspace =
						{
				new FieldUpdateRequestDto(
					Guid.Parse(DocumentFieldGuids.RelativityDestinationCase),  // TODO REL-322557
					new MultiObjectReferenceDto(destinationWorkspaceInstanceId)),
				new FieldUpdateRequestDto(
					Guid.Parse(DocumentFieldGuids.JobHistory), // TODO REL-322557
					new MultiObjectReferenceDto(jobHistoryInstanceId)),
			};
			return documentTagsInSourceWorkspace;
		}

		private async Task TagBatchOfDocumentsAsync(
			IScratchTableRepository scratchTableRepository,
			int batchSize,
			int documentsOffset,
			IEnumerable<FieldUpdateRequestDto> documentTagsInSourceWorkspace)
		{
			try
			{
				IEnumerable<int> currentBatch = ReadCurrentDocumentsIDsForBatch(scratchTableRepository, batchSize, documentsOffset);
				await MassUpdateDocumentsAsync(documentTagsInSourceWorkspace, currentBatch).ConfigureAwait(false);
			}
			catch (IntegrationPointsException ex)
			{
				_logger?.LogError(ex,
					"Error occured while tagging documents in source workspace. Number of processed items: {processedCount}",
					documentsOffset);
				throw;
			}
		}

		private static IEnumerable<int> ReadCurrentDocumentsIDsForBatch(IScratchTableRepository scratchTableRepository, int batchSize, int documentsOffset)
		{
			try
			{
				return scratchTableRepository.ReadDocumentIDs(documentsOffset, batchSize).ToList();
			}
			catch (Exception ex)
			{
				throw new IntegrationPointsException(TaggingErrors.SOURCE_OBJECT_SCRATCH_TABLE_READ_ERROR, ex);
			}
		}

		private async Task MassUpdateDocumentsAsync(IEnumerable<FieldUpdateRequestDto> documentTagsInSourceWorkspace, IEnumerable<int> currentBatch)
		{
			bool isUpdated;
			try
			{
				isUpdated = await _documentRepository
					.MassUpdateDocumentsAsync(currentBatch, documentTagsInSourceWorkspace)
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IntegrationPointsException(TaggingErrors.SOURCE_OBJECT_MASS_EDIT_FAILURE, ex)
				{
					ExceptionSource = IntegrationPointsExceptionSource.KEPLER
				};
			}

			if (!isUpdated)
			{
				throw new IntegrationPointsException(TaggingErrors.SOURCE_OBJECT_MASS_EDIT_FAILURE)
				{
					ExceptionSource = IntegrationPointsExceptionSource.KEPLER
				};
			}
		}

		private static void ValidateBatchSize(int batchSize)
		{
			if (batchSize < 1)
			{
				string errorMessage = $"Batch size for source documents tagging has to be bigger than 0, but found {batchSize}";
				throw new IntegrationPointsException(errorMessage)
				{
					ShouldAddToErrorsTab = true
				};
			}
		}

		private void LogNoDocumentsToTag()
		{
			_logger?.LogInformation("Skipping source documents tagging - no documents to tag.");
		}

		private void LogTaggingStarted(int numberOfDocuments, int batchSize)
		{
			_logger?.LogInformation(
				"Tagging documents in source workspace started. Batch size: {batchSize}, number of documents: {numberOfDocuments}",
				batchSize,
				numberOfDocuments);
		}
	}
}
