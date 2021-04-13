using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Transfer
{
	internal class FileStatisticsCalculator : IFileStatisticsCalculator
	{
		private const int _BATCH_SIZE_FOR_FILE_QUERIES = 10000;

		private readonly IStatisticsConfiguration _configuration;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IImageFileRepository _imageFileRepository;
		private readonly INativeFileRepository _nativeFileRepository;
		private readonly IRdoManager _rdoManager;
		private readonly ISyncLog _logger;

		public FileStatisticsCalculator(IStatisticsConfiguration configuration, ISourceServiceFactoryForUser serviceFactory, 
			IImageFileRepository imageFileRepository, INativeFileRepository nativeFileRepository, IRdoManager rdoManager, ISyncLog logger)
		{
			_configuration = configuration;
			_serviceFactory = serviceFactory;
			_imageFileRepository = imageFileRepository;
			_nativeFileRepository = nativeFileRepository;
			_rdoManager = rdoManager;
			_logger = logger;
		}

		/// <summary>
		/// Returns long running task
		/// </summary>
		public async Task<long> CalculateNativesTotalSizeAsync(int workspaceId, QueryRequest request, CompositeCancellationToken token)
		{
			_logger.LogInformation("Initializing calculating total natives size (in chunks of {batchSize})", _BATCH_SIZE_FOR_FILE_QUERIES);

			IEnumerable<IList<int>> documentArtifactIdBatches = await QueryDocumentsAsync(workspaceId, request).ConfigureAwait(false);

			long nativesTotalSize = 0, nativesTotalCount = 0, documentsTotalCount = 0;
			try
			{
				int batchIndex = 1;

				foreach (IList<int> batch in documentArtifactIdBatches)
				{
					if (token.IsDrainStopRequested)
					{
						_logger.LogInformation("Natives size calculation has been drain-stopped.");
						return nativesTotalSize;
					}

					_logger.LogInformation("Calculating total natives size for {documentsCount} in chunk {batchIndex}.", batch.Count, batchIndex);

					IList<INativeFile> nativesInBatch = (await _nativeFileRepository.QueryAsync(workspaceId, batch).ConfigureAwait(false)).ToList();
					nativesTotalSize += nativesInBatch.Sum(x => x.Size);
					nativesTotalCount += nativesInBatch.Count;
					documentsTotalCount += batch.Count;

					_logger.LogInformation("Calculated total natives size for {documentsCount} in chunk {batchIndex}.", batch.Count, batchIndex++);
				}
			}
			finally
			{
				await WriteDocumentsSizeStatistics(workspaceId, documentsTotalCount, nativesTotalSize, nativesTotalCount).ConfigureAwait(false);
			}

			_logger.LogInformation("Finished calculating total natives size (in chunks of {batchSize} ", _BATCH_SIZE_FOR_FILE_QUERIES);

			return nativesTotalSize;
		}

		public async Task<ImagesStatistics> CalculateImagesStatisticsAsync(int workspaceId, 
			QueryRequest request, QueryImagesOptions options, CompositeCancellationToken token)
		{
			_logger.LogInformation("Initializing calculating images totals for (in chunks of {batchSize} )", _BATCH_SIZE_FOR_FILE_QUERIES);
			
			IEnumerable<IList<int>> documentArtifactIdBatches = await QueryDocumentsAsync(workspaceId, request).ConfigureAwait(false);

			long imagesTotalCount = 0, imagesTotalSize = 0, documentsTotalCount = 0;
			try
			{
				int batchIndex = 1;
				foreach (IList<int> batch in documentArtifactIdBatches)
				{
					if (token.IsDrainStopRequested)
					{
						_logger.LogInformation("Images size calculation has been drain-stopped.");
						return new ImagesStatistics(imagesTotalCount, imagesTotalSize);
					}

					_logger.LogInformation("Calculating images totals for {documentsCount} documents in chunk {batchIndex}.", batch.Count, batchIndex);

					IEnumerable<ImageFile> imagesInBatch = await _imageFileRepository.QueryImagesForDocumentsAsync(workspaceId, batch.ToArray(), options).ConfigureAwait(false);

					foreach (ImageFile image in imagesInBatch)
					{
						imagesTotalCount++;
						imagesTotalSize += image.Size;
					}

					documentsTotalCount += batch.Count;

					_logger.LogInformation("Calculated images totals for {documentsCount} documents in chunk {batchIndex}.", batch.Count, batchIndex++);
				}
			}
			finally
			{
				await WriteDocumentsSizeStatistics(workspaceId, documentsTotalCount, imagesTotalSize, imagesTotalCount).ConfigureAwait(false);
			}

			_logger.LogInformation("Finished calculating images totals for (in chunks of {batchSize} ", _BATCH_SIZE_FOR_FILE_QUERIES);

			return new ImagesStatistics(imagesTotalCount, imagesTotalSize);
		}

		private async Task<IEnumerable<IList<int>>> QueryDocumentsAsync(int workspaceId, QueryRequest request)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				SyncStatisticsRdo syncStatistics = await _rdoManager.GetAsync<SyncStatisticsRdo>(workspaceId, _configuration.SyncStatisticsId).ConfigureAwait(false);

				List<RelativityObjectSlim> documents;
				if (syncStatistics.RunId == Guid.Empty)
				{
					ExportInitializationResults exportInitializationResults = await objectManager.InitializeExportAsync(workspaceId, request, 1).ConfigureAwait(false);
					int exportedRecordsCount = (int)exportInitializationResults.RecordCount;
					
					documents = await objectManager.QueryAllByExportRunId(workspaceId, 
							exportInitializationResults.RunID, 0, exportedRecordsCount)
						.ConfigureAwait(false);

					syncStatistics.RunId = exportInitializationResults.RunID;
					syncStatistics.DocumentsRequested = exportedRecordsCount;

					await _rdoManager.SetValuesAsync(workspaceId, syncStatistics).ConfigureAwait(false);
				}
				else
				{
					documents = await objectManager.QueryAllByExportRunId(workspaceId, syncStatistics.RunId,
							(int)syncStatistics.DocumentsCalculated, (int)syncStatistics.DocumentsRequested)
						.ConfigureAwait(false);
				}

				IEnumerable<IList<int>> documentArtifactIdBatches = documents
					.Select(x => x.ArtifactID)
					.SplitList(_BATCH_SIZE_FOR_FILE_QUERIES);

				return documentArtifactIdBatches;
			}
		}

		private async Task WriteDocumentsSizeStatistics(int workspaceId, long documentsTotalCount, long filesTotalSize, long filesTotalCount)
		{
			SyncStatisticsRdo syncStatistics = await _rdoManager.GetAsync<SyncStatisticsRdo>(workspaceId, _configuration.SyncStatisticsId).ConfigureAwait(false);

			syncStatistics.DocumentsCalculated += documentsTotalCount;
			syncStatistics.FilesSizeCalculated += filesTotalSize;
			syncStatistics.FilesCountCalculated += filesTotalCount;

			await _rdoManager.SetValuesAsync(workspaceId, syncStatistics).ConfigureAwait(false);
		}
	}
}