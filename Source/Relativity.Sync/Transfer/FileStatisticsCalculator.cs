using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Transfer
{
	internal class FileStatisticsCalculator : IFileStatisticsCalculator
	{
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
			SyncStatisticsRdo result = await CalculateFilesTotalSizeAsync(workspaceId, request,
				async batch => await CalculateNativesSize(workspaceId, batch), token).ConfigureAwait(false);

			return result.FilesSizeCalculated;
		}

		public async Task<ImagesStatistics> CalculateImagesStatisticsAsync(int workspaceId,
			QueryRequest request, QueryImagesOptions options, CompositeCancellationToken token)
		{
			SyncStatisticsRdo result = await CalculateFilesTotalSizeAsync(workspaceId, request,
				async batch => await CalculateImagesSize(workspaceId, batch, options), token).ConfigureAwait(false);

			return new ImagesStatistics(result.FilesCountCalculated, result.FilesSizeCalculated);
		}

		private async Task<SyncStatisticsRdo> CalculateFilesTotalSizeAsync(int workspaceId, QueryRequest request, 
			Func<IList<int>, Task<FileSizeResult>> filesCalculationFunc, CompositeCancellationToken token)
		{
			SyncStatisticsRdo syncStatistics;
			try
			{
				_logger.LogInformation("Initializing calculating total files size (in chunks of {batchSize})",
					_configuration.BatchSizeForFileQueries);

				syncStatistics = await InitializeDocumentsQuery(workspaceId, request).ConfigureAwait(false);
				try
				{
					int batchIndex = 1;

					IList<int> batch;
					while ((batch = await GetNextBatch(workspaceId, syncStatistics).ConfigureAwait(false)) != null)
					{
						_logger.LogInformation("Calculating total files size for {documentsCount} in chunk {batchIndex}.",
							batch.Count, batchIndex);

						FileSizeResult result = await filesCalculationFunc(batch).ConfigureAwait(false);

						syncStatistics.FilesSizeCalculated += result.FilesSize;
						syncStatistics.FilesCountCalculated += result.FilesCount;
						syncStatistics.DocumentsCalculated += result.DocumentsCount;

						_logger.LogInformation("Calculated total files size for {documentsCount} in chunk {batchIndex}.",
							batch.Count, batchIndex++);

						if (token.IsDrainStopRequested)
						{
							_logger.LogInformation("Files size calculation has been drain-stopped.");
							break;
						}
					}
				}
				finally
				{
					await _rdoManager.SetValuesAsync(workspaceId, syncStatistics).ConfigureAwait(false);
				}

				_logger.LogInformation("Finished calculating total files size (in chunks of {batchSize} ",
					_configuration.BatchSizeForFileQueries);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during statistics calculation. Empty values has been returned.");
				syncStatistics = new SyncStatisticsRdo();
			}

			return syncStatistics;
		}

		private async Task<FileSizeResult> CalculateNativesSize(int workspaceId, IList<int> batch)
		{
			IList<INativeFile> nativesInBatch = (await _nativeFileRepository.QueryAsync(workspaceId, batch).ConfigureAwait(false)).ToList();

			return new FileSizeResult
			{
				DocumentsCount = batch.Count,
				FilesCount = nativesInBatch.Count,
				FilesSize = nativesInBatch.Sum(x => x.Size)
			};
		}

		private async Task<FileSizeResult> CalculateImagesSize(int workspaceId, IList<int> batch, QueryImagesOptions options)
		{
			IList<ImageFile> imagesInBatch = (await _imageFileRepository
					.QueryImagesForDocumentsAsync(workspaceId, batch.ToArray(), options).ConfigureAwait(false))
				.ToList();

			return new FileSizeResult
			{
				DocumentsCount = batch.Count,
				FilesCount = imagesInBatch.Count(),
				FilesSize = imagesInBatch.Sum(x => x.Size)
			};
		}

		private async Task<SyncStatisticsRdo> InitializeDocumentsQuery(int workspaceId, QueryRequest request)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				SyncStatisticsRdo syncStatistics = await _rdoManager.GetAsync<SyncStatisticsRdo>(workspaceId, _configuration.SyncStatisticsId).ConfigureAwait(false);

				if (syncStatistics.RunId == Guid.Empty)
				{
					ExportInitializationResults exportInitializationResults = await objectManager.InitializeExportAsync(workspaceId, request, 1).ConfigureAwait(false);
					int exportedRecordsCount = (int)exportInitializationResults.RecordCount;

					syncStatistics.RunId = exportInitializationResults.RunID;
					syncStatistics.DocumentsRequested = exportedRecordsCount;

					await _rdoManager.SetValuesAsync(workspaceId, syncStatistics).ConfigureAwait(false);
				}

				return syncStatistics;
			}
		}

		private async Task<IList<int>> GetNextBatch(int workspaceId, SyncStatisticsRdo statistics)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				int blockSize = Math.Min(
					(int)(statistics.DocumentsRequested - statistics.DocumentsCalculated),
					_configuration.BatchSizeForFileQueries);

				RelativityObjectSlim[] exportResultsBlock = await objectManager
					.RetrieveResultsBlockFromExportAsync(workspaceId, statistics.RunId,
						blockSize, (int)statistics.DocumentsCalculated)
					.ConfigureAwait(false);

				return exportResultsBlock != null && exportResultsBlock.Any()
					? exportResultsBlock.Select(x => x.ArtifactID).ToList()
					: null;
			}
		}

		private class FileSizeResult
		{
			public long DocumentsCount { get; set; }
			public long FilesCount { get; set; }
			public long FilesSize { get; set; }
		}
	}
}