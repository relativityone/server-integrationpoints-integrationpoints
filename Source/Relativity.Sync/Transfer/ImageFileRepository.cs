﻿using kCura.WinEDDS.Service.Export;
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
		private const string _FILENAME_COLUMN_NAME_PRODUCTION = "ImageFileName";
		private const string _SIZE_COLUMN_NAME_PRODUCTION = "ImageSize";
		private const string _NATIVE_IDENTIFIER = "NativeIdentifier";

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

		public async Task<IEnumerable<ImageFile>> QueryImagesForDocumentsAsync(int workspaceId, int[] documentIds, QueryImagesOptions options)
		{
			IEnumerable<ImageFile> empty = Enumerable.Empty<ImageFile>();

			if (documentIds == null || !documentIds.Any())
			{
				return empty;
			}

			_logger.LogInformation("Searching for image files based on options. Documents count: {numberOfDocuments}", documentIds.Length);

			using (ISearchManager searchManager = await _searchManagerFactory.CreateSearchManagerAsync().ConfigureAwait(false))
			{
				ImageFile[] imageFiles = options != null && options.ProductionImagePrecedence
					? RetrieveImagesByProductionsForDocuments(searchManager, workspaceId, documentIds, options)
					: RetrieveOriginalImagesForDocuments(searchManager, workspaceId, documentIds).Item1;

				_logger.LogInformation("Found {numberOfImages} image files for {numberOfDocuments} documents", 
					imageFiles.Length,
					imageFiles.Select(x => x.DocumentArtifactId).Distinct().Count());

				return imageFiles;
			}
		}

		public async Task<long> CalculateImagesTotalSizeAsync(int workspaceId, QueryRequest request, QueryImagesOptions options)
		{
			_logger.LogInformation("Initializing calculating total image size (in chunks of {batchSize} )", _BATCH_SIZE_FOR_IMAGES_SIZE_QUERIES);
			long imagesTotalSize = 0;
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				IEnumerable<IList<int>> documentArtifactIdBatches = (await objectManager.QueryAllAsync(workspaceId, request).ConfigureAwait(false))
					.Select(x => x.ArtifactID)
					.SplitList(_BATCH_SIZE_FOR_IMAGES_SIZE_QUERIES);

				foreach (IList<int> batch in documentArtifactIdBatches)
				{
					IEnumerable<ImageFile> imagesInBatch = await QueryImagesForDocumentsAsync(workspaceId, batch.ToArray(), options).ConfigureAwait(false);
					imagesTotalSize += imagesInBatch.Sum(x => x.Size);
				}
			}
			return imagesTotalSize;
		}

		private ImageFile[] RetrieveImagesByProductionsForDocuments(ISearchManager searchManager, int workspaceId, int[] documentIds, QueryImagesOptions options)
		{
			(ImageFile[] producedImageFiles, int[] documentsWithoutImages) = GetImagesWithPrecedence(workspaceId, searchManager, options.ProductionIds, documentIds);

			if (!producedImageFiles.Any())
			{
				_logger.LogWarning("No produced images found for productions [{productionIds}]", string.Join(",", options.ProductionIds));
			}

			ImageFile[] originalImageFiles = Array.Empty<ImageFile>();

			if (options.IncludeOriginalImageIfNotFoundInProductions)
			{
				(originalImageFiles, documentsWithoutImages) = RetrieveOriginalImagesForDocuments(searchManager, workspaceId, documentsWithoutImages);
			}

			_logger.LogInformation("Image retrieve statistics: ProducedImages - {producedImagesCount}, OriginalImages - {originalImagesCount}, DocumentsWithoutImages {noImagesCount}",
				producedImageFiles.Length, originalImageFiles.Length, documentsWithoutImages.Length);

			return producedImageFiles.Concat(originalImageFiles).ToArray();
		}

		private (ImageFile[], int[]) GetImagesWithPrecedence(int workspaceId, ISearchManager searchManager, int[] productionPrecedence, int[] documentIds)
		{
			var result = new Dictionary<int, IEnumerable<ImageFile>>();

			int[] documentsWithoutImages = documentIds;

			foreach (int production in productionPrecedence)
			{
				EnumerableRowCollection<ImageFile> producedImages = searchManager
					.RetrieveImagesForProductionDocuments(workspaceId, documentsWithoutImages, production)
					.Tables[0]
					.AsEnumerable()
					.Select(x => GetImageFileFromProduction(x, production));

				ILookup<int, ImageFile> imagesPerDocument = producedImages.ToLookup(
					x => x.DocumentArtifactId,
					x => x);

				foreach (IGrouping<int, ImageFile> documentImages in imagesPerDocument)
				{
					if (!result.ContainsKey(documentImages.Key))
					{
						result.Add(documentImages.Key, documentImages);
					}
				}

				documentsWithoutImages = documentsWithoutImages.Where(x => !result.ContainsKey(x)).ToArray();
			}

			return (result.Values.SelectMany(x => x).ToArray(), documentsWithoutImages);
		}

		private (ImageFile[], int[]) RetrieveOriginalImagesForDocuments(ISearchManager searchManager, int workspaceId, int[] documentIds)
		{
			DataSet dataSet = searchManager.RetrieveImagesForDocuments(workspaceId, documentIds);

			if (dataSet == null || dataSet.Tables.Count == 0)
			{
				_logger.LogWarning("SearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport returned null/empty data set.");
				_logger.LogInformation("Image retrieve statistics: OriginalImages - {originalImagesCount}, DocumentsWithoutImages {noImagesCount}",
					0, documentIds.Length);
				return (Array.Empty<ImageFile>(), documentIds.ToArray());
			}

			ImageFile[] imageFiles = dataSet.Tables[0].AsEnumerable().Select(GetImageFile).ToArray();

			_logger.LogInformation("Image retrieve statistics: OriginalImages - {originalImagesCount}, DocumentsWithoutImages {noImagesCount}",
				imageFiles.Length, documentIds.Length - imageFiles.Length);

			return (imageFiles, documentIds.Except(imageFiles.Select(x => x.DocumentArtifactId)).ToArray());
		}

		private ImageFile GetImageFile(DataRow dataRow)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME);

			return new ImageFile(documentArtifactId, location, fileName, size);
		}

		private ImageFile GetImageFileFromProduction(DataRow dataRow, int productionId)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME_PRODUCTION);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME_PRODUCTION);
			string nativeIdentifier = GetValue<string>(dataRow, _NATIVE_IDENTIFIER);
		
			return new ImageFile(documentArtifactId, location, fileName, size, productionId, nativeIdentifier);
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
