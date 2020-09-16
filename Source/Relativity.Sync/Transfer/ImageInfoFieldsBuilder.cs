using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoFieldsBuilder : IImageSpecialFieldBuilder
	{
		private readonly IImageFileRepository _imageFileRepository;
		private readonly IImageRetrieveConfiguration _configuration;

		public ImageInfoFieldsBuilder(IImageFileRepository imageFileRepository, IImageRetrieveConfiguration configuration)
		{
			_imageFileRepository = imageFileRepository;
			_configuration = configuration;
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

			var imageFiles = await _imageFileRepository.QueryImagesForDocumentsAsync(sourceWorkspaceArtifactId, documentArtifactIds.ToList(), options)
				.ConfigureAwait(false);

			var imageFilesLookup = imageFiles.ToLookup(x => x.DocumentArtifactId, x => x);
			var documentToImageFiles = documentArtifactIds.ToDictionary(x => x, x => imageFilesLookup[x]);

			return new ImageInfoRowValuesBuilder(documentToImageFiles);
		}
	}
}
