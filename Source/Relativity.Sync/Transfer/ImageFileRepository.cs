using kCura.WinEDDS.Service.Export;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal class ImageFileRepository : IImageFileRepository
	{
		private const int _BATCH_SIZE_FOR_IMAGES_SIZE_QUERIES = 10000;
		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _SIZE_COLUMN_NAME = "Size";

		private readonly ISearchManagerFactory _searchManagerFactory;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public ImageFileRepository(ISearchManagerFactory searchManagerFactory, ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_searchManagerFactory = searchManagerFactory;
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<IEnumerable<ImageFile>> QueryImagesForDocumentsAsync(int workspaceId, IList<int> documentIds, QueryImagesOptions options)
		{
			IEnumerable<ImageFile> empty = Enumerable.Empty<ImageFile>();

			if (documentIds == null || !documentIds.Any())
			{
				return empty;
			}

			_logger.LogInformation("Searching for image files based on options. Documents count: {numberOfDocuments}", documentIds.Count);

			using (ISearchManager searchManager = await _searchManagerFactory.CreateSearchManagerAsync().ConfigureAwait(false))
			{
				IList<ImageFile> imageFiles = options != null && options.ProductionImagePrecedence
					? RetrieveImagesByProductionsForDocuments(searchManager, workspaceId, documentIds, options)
					: RetrieveOriginalImagesForDocuments(searchManager, workspaceId, documentIds);

				_logger.LogInformation("Found {numberOfImages} image files.", imageFiles.Count);

				return imageFiles;
			}
		}

		public async Task<long> CalculateImagesTotalSizeAsync(int workspaceId, QueryRequest request, QueryImagesOptions options)
		{
			_logger.LogInformation("Initializing calculating total natives size (in chunks of {batchSize} )", _BATCH_SIZE_FOR_IMAGES_SIZE_QUERIES);
			long imagesTotalSize = 0;
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				IEnumerable<IList<int>> documentArtifactIdBatches = (await objectManager.QueryAllAsync(workspaceId, request).ConfigureAwait(false))
					.Select(x => x.ArtifactID)
					.SplitList(_BATCH_SIZE_FOR_IMAGES_SIZE_QUERIES);

				foreach (IList<int> batch in documentArtifactIdBatches)
				{
					IEnumerable<ImageFile> imagesInBatch = await QueryImagesForDocumentsAsync(workspaceId, batch, options).ConfigureAwait(false);
					imagesTotalSize += imagesInBatch.Sum(x => x.Size);
				}
			}
			return imagesTotalSize;
		}

		private IList<ImageFile> RetrieveImagesByProductionsForDocuments(ISearchManager searchManager, int workspaceId, IList<int> documentIds, QueryImagesOptions options)
		{
			int producedImagesCount = 0, originalImagesCount = 0, documentsWithoutImagesCount = documentIds.Count;

			var dataSet = searchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport(workspaceId, options.ProductionIds.ToArray(), documentIds.ToArray());

			if (dataSet == null || dataSet.Tables.Count == 0)
			{
				_logger.LogWarning("SearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport returned null/empty data set.");
				_logger.LogInformation("Image retrieve statistics: ProducedImages - {producedImagesCount}, OriginalImages - {originalImagesCount}, DocumentsWithoutImages {noImagesCount}",
					producedImagesCount, originalImagesCount, documentsWithoutImagesCount);
				return new List<ImageFile>();
			}

			var imageFiles = dataSet.Tables[0].AsEnumerable().Select(x => GetImageFile(x)).ToList();
			producedImagesCount += imageFiles.Count;

			if(options.IncludeOriginalImageIfNotFoundInProductions)
			{
				var documentIdsForOriginalImages = documentIds.Except(imageFiles.Select(x => x.DocumentArtifactId)).ToList();
				var originalImageFiles = RetrieveOriginalImagesForDocuments(searchManager, workspaceId, documentIdsForOriginalImages).ToList();

				originalImagesCount += originalImageFiles.Count;

				imageFiles.AddRange(originalImageFiles);
			}

			documentsWithoutImagesCount -= imageFiles.Count;

			_logger.LogInformation("Image retrieve statistics: ProducedImages - {producedImagesCount}, OriginalImages - {originalImagesCount}, DocumentsWithoutImages {noImagesCount}",
				producedImagesCount, originalImagesCount, documentsWithoutImagesCount);

			return imageFiles;
		}

		private IList<ImageFile> RetrieveOriginalImagesForDocuments(ISearchManager searchManager, int workspaceId, IList<int> documentIds)
		{
			var dataSet = searchManager.RetrieveImagesForDocuments(workspaceId, documentIds.ToArray());

			if (dataSet == null || dataSet.Tables.Count == 0)
			{
				_logger.LogWarning("SearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport returned null/empty data set.");
				_logger.LogInformation("Image retrieve statistics: OriginalImages - {originalImagesCount}, DocumentsWithoutImages {noImagesCount}",
					0, documentIds.Count);
				return new List<ImageFile>();
			}

			var imageFiles = dataSet.Tables[0].AsEnumerable().Select(x => GetImageFile(x)).ToList();

			_logger.LogInformation("Image retrieve statistics: OriginalImages - {originalImagesCount}, DocumentsWithoutImages {noImagesCount}",
				imageFiles.Count, documentIds.Count - imageFiles.Count);

			return imageFiles;
		}

		private ImageFile GetImageFile(DataRow dataRow)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME);

			return new ImageFile(documentArtifactId, location, fileName, size);
		}

		private T GetValue<T>(DataRow row, string columnName)
		{
			object value = null;
			try
			{
				value = row[columnName];
				return (T)value;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving image file info from column \"{columnName}\". Value: \"{value}\" Requested type: \"{type}\"", columnName, value, typeof(T));
				throw;
			}
		}
	}
}
