using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal class ImageFileRepository : IImageFileRepository
	{
		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _FILENAME_COLUMN_NAME_PRODUCTION = "ImageFileName";
		private const string _SIZE_COLUMN_NAME_PRODUCTION = "ImageSize";
		private const string _NATIVE_IDENTIFIER = "NativeIdentifier";
		private const string _IDENTIFIER = "Identifier";

		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _SIZE_COLUMN_NAME = "Size";

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;
		private readonly SyncJobParameters _parameters;

		public ImageFileRepository(ISourceServiceFactoryForUser serviceFactory, ISyncLog logger, SyncJobParameters parameters)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
			_parameters = parameters;
		}

		public async Task<IEnumerable<ImageFile>> QueryImagesForDocumentsAsync(int workspaceId, int[] documentIds,
			QueryImagesOptions options)
		{
			IEnumerable<ImageFile> empty = Enumerable.Empty<ImageFile>();

			if (documentIds == null || !documentIds.Any())
			{
				_logger.LogWarning("Empty documents list has been provided. Searching images won't be executed.");
				return empty;
			}

			_logger.LogInformation("Searching for image files based on options. Documents count: {numberOfDocuments}", documentIds.Length);

			using (ISearchService searchManager = await _serviceFactory.CreateProxyAsync<ISearchService>().ConfigureAwait(false))
			{
				ImageFile[] imageFiles = options != null && options.ProductionImagePrecedence
					? await RetrieveImagesByProductionsForDocuments(searchManager, workspaceId, documentIds, options).ConfigureAwait(false)
					: (await RetrieveOriginalImagesForDocuments(searchManager, workspaceId, documentIds).ConfigureAwait(false)).Images;



				_logger.LogInformation("Found {numberOfImages} image files for {numberOfDocuments} documents",
					imageFiles.Length,
					imageFiles.Select(x => x.DocumentArtifactId).Distinct().Count());

				return imageFiles;
			}
		}

		private async Task<ImageFile[]> RetrieveImagesByProductionsForDocuments(ISearchService searchService,
			int workspaceId, int[] documentIds, QueryImagesOptions options)
		{
			(ImageFile[] producedImageFiles, int[] documentsWithoutImages) = await GetImagesWithProductionPrecedence(searchService, workspaceId, options.ProductionIds, documentIds).ConfigureAwait(false);

			if (!producedImageFiles.Any())
			{
				_logger.LogWarning("No produced images found for productions [{productionIds}]", string.Join(",", options.ProductionIds));
			}

			ImageFile[] originalImageFiles = Array.Empty<ImageFile>();

			if (options.IncludeOriginalImageIfNotFoundInProductions && documentsWithoutImages.Any())
			{
				(originalImageFiles, documentsWithoutImages) = await RetrieveOriginalImagesForDocuments(searchService, workspaceId, documentsWithoutImages).ConfigureAwait(false);
			}

			_logger.LogInformation("Image retrieve statistics: ProducedImages: {producedImagesCount}, OriginalImages: {originalImagesCount}, DocumentsWithoutImages: {noImagesCount}",
				producedImageFiles.Length, originalImageFiles.Length, documentsWithoutImages.Length);

			return producedImageFiles.Concat(originalImageFiles).ToArray();
		}

		private async Task<(ImageFile[], int[])> GetImagesWithProductionPrecedence(ISearchService searchService,
			int workspaceId, int[] productionPrecedence, int[] documentIds)
		{
			var result = new Dictionary<int, IEnumerable<ImageFile>>();

			int[] documentsWithoutImages = documentIds;
			foreach (int production in productionPrecedence)
			{
				DataSetWrapper dataSetWrapper = await searchService
					.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync(
						workspaceId, production, documentsWithoutImages, _parameters.WorkflowId)
					.ConfigureAwait(false);

				if (dataSetWrapper == null)
				{
					// there are no results for this production
					continue;
				}
				
				EnumerableRowCollection<ImageFile> producedImages = dataSetWrapper.Unwrap()
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

				if (documentsWithoutImages.Length == 0)
				{
					break;
				}
			}

			return (result.Values.SelectMany(x => x).ToArray(), documentsWithoutImages);
		}

		private async Task<(ImageFile[] Images, int[] DocumentWithoutImages)> RetrieveOriginalImagesForDocuments(
			ISearchService searchService, int workspaceId, int[] documentIds)
		{
			DataSetWrapper dataSet = await searchService.RetrieveImagesForSearchAsync(workspaceId, documentIds, _parameters.WorkflowId);

			if (dataSet == null || dataSet.Unwrap().Tables.Count == 0)
			{
				_logger.LogWarning("ISearchService.RetrieveImagesForSearchAsync returned null/empty data set.");
				_logger.LogInformation("Image retrieve statistics: OriginalImages: {originalImagesCount}, DocumentsWithoutImages: {noImagesCount}",
					0, documentIds.Length);
				return (Array.Empty<ImageFile>(), documentIds.ToArray());
			}

			ImageFile[] imageFiles = dataSet.Unwrap().Tables[0].AsEnumerable().Select(GetImageFile).ToArray();

			_logger.LogInformation("Image retrieve statistics: OriginalImages: {originalImagesCount}, DocumentsWithoutImages: {noImagesCount}",
				imageFiles.Length, documentIds.Length - imageFiles.Length);

			return (imageFiles, documentIds.Except(imageFiles.Select(x => x.DocumentArtifactId)).ToArray());
		}

		private ImageFile GetImageFile(DataRow dataRow)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME);

			string identifier = GetValue<string>(dataRow, _IDENTIFIER);


			return new ImageFile(documentArtifactId, identifier, location, fileName, size);
		}

		private ImageFile GetImageFileFromProduction(DataRow dataRow, int productionId)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME_PRODUCTION);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME_PRODUCTION);
			string nativeIdentifier = GetValue<string>(dataRow, _NATIVE_IDENTIFIER);

			return new ImageFile(documentArtifactId, nativeIdentifier, location, fileName, size, productionId);
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
