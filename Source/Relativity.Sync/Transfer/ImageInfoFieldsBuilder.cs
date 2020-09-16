using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoFieldsBuilder : IImageSpecialFieldBuilder
	{
		private readonly IImageFileRepository _imageFileRepository;
		private readonly IImageRetrieveConfiguration _configuration;
		private readonly ISyncLog _logger;

		public ImageInfoFieldsBuilder(IImageFileRepository imageFileRepository, IImageRetrieveConfiguration configuration, ISyncLog logger)
		{
			_imageFileRepository = imageFileRepository;
			_configuration = configuration;
			_logger = logger;
		}

		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			yield return FieldInfoDto.ImageFileNameField();
			yield return FieldInfoDto.ImageFileLocationField();
		}

		public async Task<IImageSpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			QueryImagesOptions options = new QueryImagesOptions
			{
				ProductionIds = _configuration.ProductionIds,
				IncludeOriginalImageIfNotFoundInProductions = _configuration.IncludeOriginalImageIfNotFoundInProductions
			};

			var imageFiles = (await _imageFileRepository.QueryImagesForDocumentsAsync(sourceWorkspaceArtifactId, documentArtifactIds.ToList(), options)
				.ConfigureAwait(false)).ToList();

			LogWarningIfImagesFoundForDocumentsNotSelectedToSync(imageFiles, documentArtifactIds);

			var imageFilesLookup = imageFiles.ToLookup(x => x.DocumentArtifactId, x => x);
			var documentToImageFiles = documentArtifactIds.ToDictionary(x => x, x => imageFilesLookup[x]);

			return new ImageInfoRowValuesBuilder(documentToImageFiles);
		}

		private void LogWarningIfImagesFoundForDocumentsNotSelectedToSync(IEnumerable<ImageFile> imageFiles, IEnumerable<int> documentArtifactIds)
		{
			var retrievedImagesNotFoundInDocumentsToSync = imageFiles.Select(x => x.DocumentArtifactId).Except(documentArtifactIds).ToList();
			if (retrievedImagesNotFoundInDocumentsToSync.Count > 0)
			{
				_logger.LogWarning("Images has been retrieved for documents which are not selected to push. Potential data loss. Documents: {documents}",
					string.Join(",", retrievedImagesNotFoundInDocumentsToSync));
			}
		}
	}
}
