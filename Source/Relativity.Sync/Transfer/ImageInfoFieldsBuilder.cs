using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoFieldsBuilder : IImageSpecialFieldBuilder
	{
		private readonly IImageFileRepository _imageFileRepository;
		private readonly IImageRetrieveConfiguration _configuration;
		private readonly IAntiMalwareHandler _antiMalwareHandler;
		private readonly IAPILog _logger;

		public ImageInfoFieldsBuilder(IImageFileRepository imageFileRepository, IImageRetrieveConfiguration configuration, IAntiMalwareHandler antiMalwareHandler, IAPILog logger)
		{
			_imageFileRepository = imageFileRepository;
			_configuration = configuration;
			_antiMalwareHandler = antiMalwareHandler;
			_logger = logger;
		}

		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			yield return FieldInfoDto.ImageFileNameField();
			yield return FieldInfoDto.ImageFileLocationField();
			yield return FieldInfoDto.ImageIdentifierField();
		}

		public async Task<IImageSpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds)
		{
			QueryImagesOptions options = new QueryImagesOptions
			{
				ProductionIds = _configuration.ProductionImagePrecedence,
				IncludeOriginalImageIfNotFoundInProductions = _configuration.IncludeOriginalImageIfNotFoundInProductions
			};

			List<ImageFile> imageFiles = (await _imageFileRepository.QueryImagesForDocumentsAsync(sourceWorkspaceArtifactId, documentArtifactIds, options)
				.ConfigureAwait(false)).ToList();

			LogWarningIfImagesFoundForDocumentsNotSelectedToSync(imageFiles, documentArtifactIds);

			ILookup<int, ImageFile> imageFilesLookup = imageFiles.ToLookup(x => x.DocumentArtifactId, x => x);
			Dictionary<int, ImageFile[]> documentToImageFiles = documentArtifactIds.ToDictionary(x => x, x => imageFilesLookup[x].ToArray());

			return new ImageInfoRowValuesBuilder(documentToImageFiles, _antiMalwareHandler);
		}

		private void LogWarningIfImagesFoundForDocumentsNotSelectedToSync(IEnumerable<ImageFile> imageFiles, IEnumerable<int> documentArtifactIds)
		{
			List<int> retrievedImagesNotFoundInDocumentsToSync = imageFiles.Select(x => x.DocumentArtifactId).Except(documentArtifactIds).ToList();
			if (retrievedImagesNotFoundInDocumentsToSync.Count > 0)
			{
				_logger.LogWarning("Images has been retrieved for documents which are not selected to push. Potential data loss. Documents: {documents}",
					string.Join(",", retrievedImagesNotFoundInDocumentsToSync));
			}
		}
	}
}
