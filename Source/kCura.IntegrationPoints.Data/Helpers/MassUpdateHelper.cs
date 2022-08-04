using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Helpers
{
    public class MassUpdateHelper : IMassUpdateHelper
    {
        private readonly IConfig _config;
        private readonly IAPILog _logger;

        public MassUpdateHelper(IConfig config, IAPILog logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = (logger ?? throw new ArgumentNullException(nameof(logger)))
                .ForContext<MassUpdateHelper>();
        }

        public Task UpdateArtifactsAsync(
            IScratchTableRepository artifactsToUpdateRepository,
            FieldUpdateRequestDto[] fieldsToUpdate,
            IRepositoryWithMassUpdate repositoryWithMassUpdate)
        {
            if (artifactsToUpdateRepository == null)
            {
                throw new ArgumentNullException(nameof(artifactsToUpdateRepository));
            }

            ISourceArtifactsReader sourceArtifactsReader = new ScratchTableReader(artifactsToUpdateRepository);
            return UpdateArtifactsAsync(
                sourceArtifactsReader,
                fieldsToUpdate,
                repositoryWithMassUpdate);
        }

        public Task UpdateArtifactsAsync(
            ICollection<int> artifactsToUpdate,
            FieldUpdateRequestDto[] fieldsToUpdate,
            IRepositoryWithMassUpdate repositoryWithMassUpdate)
        {
            if (artifactsToUpdate == null)
            {
                throw new ArgumentNullException(nameof(artifactsToUpdate));
            }

            ISourceArtifactsReader sourceArtifactsReader = new CollectionReader(artifactsToUpdate);
            return UpdateArtifactsAsync(
                sourceArtifactsReader,
                fieldsToUpdate,
                repositoryWithMassUpdate);
        }

        private async Task UpdateArtifactsAsync(
            ISourceArtifactsReader sourceArtifactsReader,
            FieldUpdateRequestDto[] fieldsToUpdate,
            IRepositoryWithMassUpdate repositoryWithMassUpdate)
        {
            int numberOfArtifactsToUpdate = sourceArtifactsReader.GetCount();
            if (numberOfArtifactsToUpdate <= 0)
            {
                LogNoArtifactsToUpdate();
                return;
            }

            int batchSize = ReadBatchSizeFromConfigAndValidateValue();

            LogMassUpdateStarted(numberOfArtifactsToUpdate, batchSize);

            for (int processedCount = 0; processedCount < numberOfArtifactsToUpdate; processedCount += batchSize)
            {
                await UpdateBatchOfArtifactsAsync(
                        sourceArtifactsReader,
                        batchSize,
                        processedCount,
                        fieldsToUpdate,
                        repositoryWithMassUpdate)
                    .ConfigureAwait(false);
            }
        }

        private async Task UpdateBatchOfArtifactsAsync(
            ISourceArtifactsReader sourceArtifactsReader,
            int batchSize,
            int documentsOffset,
            IEnumerable<FieldUpdateRequestDto> fieldsToUpdate,
            IRepositoryWithMassUpdate repositoryWithMassUpdate)
        {
            try
            {
                IEnumerable<int> currentBatch = sourceArtifactsReader.ReadArtifactIDs(documentsOffset, batchSize);
                await MassUpdateArtifactsAsync(fieldsToUpdate, currentBatch, repositoryWithMassUpdate)
                    .ConfigureAwait(false);
            }
            catch (IntegrationPointsException ex)
            {
                _logger.LogError(ex,
                    "Error occured while mass updating artifacts. Number of processed items: {processedCount}",
                    documentsOffset);
                throw;
            }
        }

        private static async Task MassUpdateArtifactsAsync(
            IEnumerable<FieldUpdateRequestDto> fieldsToUpdate,
            IEnumerable<int> currentBatch,
            IRepositoryWithMassUpdate repositoryWithMassUpdate)
        {
            bool isUpdated;
            try
            {
                isUpdated = await repositoryWithMassUpdate
                    .MassUpdateAsync(currentBatch, fieldsToUpdate)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new IntegrationPointsException(MassEditErrors.OBJECT_MANAGER_ERROR, ex)
                {
                    ExceptionSource = IntegrationPointsExceptionSource.KEPLER
                };
            }

            ThrowIfArtifactsNotUpdated(isUpdated);
        }

        private int ReadBatchSizeFromConfigAndValidateValue()
        {
            int batchSize = _config.MassUpdateBatchSize;
            ValidateBatchSize(batchSize);
            return batchSize;
        }

        private static void ValidateBatchSize(int batchSize)
        {
            if (batchSize < 1)
            {
                string errorMessage = $"Batch size for mass update has to be bigger than 0, but found {batchSize}";
                throw new IntegrationPointsException(errorMessage)
                {
                    ShouldAddToErrorsTab = true
                };
            }
        }

        private static void ThrowIfArtifactsNotUpdated(bool isUpdated)
        {
            if (!isUpdated)
            {
                throw new IntegrationPointsException(MassEditErrors.OBJECT_MANAGER_ERROR)
                {
                    ExceptionSource = IntegrationPointsExceptionSource.KEPLER
                };
            }
        }

        private void LogNoArtifactsToUpdate()
        {
            _logger.LogInformation("Skipping mass update - no artifacts to edit.");
        }

        private void LogMassUpdateStarted(int numberOfDocuments, int batchSize)
        {
            _logger.LogInformation(
                "Mass update of artifacts started. Batch size: {batchSize}, number of documents: {numberOfDocuments}",
                batchSize,
                numberOfDocuments);
        }

        private interface ISourceArtifactsReader
        {
            int GetCount();
            IEnumerable<int> ReadArtifactIDs(int artifactsOffset, int batchSize);
        }

        private class ScratchTableReader : ISourceArtifactsReader
        {
            private readonly IScratchTableRepository _scratchTableRepository;

            public ScratchTableReader(IScratchTableRepository scratchTableRepository)
            {
                _scratchTableRepository = scratchTableRepository;
            }

            public int GetCount() => _scratchTableRepository.GetCount();

            public IEnumerable<int> ReadArtifactIDs(int artifactsOffset, int batchSize)
            {
                try
                {
                    return _scratchTableRepository.ReadArtifactIDs(artifactsOffset, batchSize);
                }
                catch (Exception ex)
                {
                    throw new IntegrationPointsException(MassEditErrors.SCRATCH_TABLE_READ_ERROR, ex);
                }
            }
        }

        private class CollectionReader : ISourceArtifactsReader
        {
            private readonly ICollection<int> _sourceCollection;

            public CollectionReader(ICollection<int> sourceCollection)
            {
                _sourceCollection = sourceCollection;
            }

            public int GetCount() => _sourceCollection.Count;

            public IEnumerable<int> ReadArtifactIDs(int artifactsOffset, int batchSize) =>
                _sourceCollection.Skip(artifactsOffset).Take(batchSize);
        }
    }
}
