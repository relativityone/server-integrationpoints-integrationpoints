using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal abstract class ImageDataSourceSnapshotExecutorBase
	{
		private const int _HAS_IMAGES_YES_CHOICE = 1034243;
		private const string _HAS_IMAGES_FIELD_NAME = "Has Images";
		private const string _PRODUCTION_IMAGE_COUNT_FIELD_NAME = "Production::Image Count";
		
		private readonly IImageFileRepository _imageFileRepository;

		protected ImageDataSourceSnapshotExecutorBase(IImageFileRepository imageFileRepository)
		{
			_imageFileRepository = imageFileRepository;
		}

		protected string CreateConditionToRetrieveImages(int[] productionIds)
		{
			string imageCondition = productionIds.Any()
				? $"('{_PRODUCTION_IMAGE_COUNT_FIELD_NAME}' > 0)"
				: $"('{_HAS_IMAGES_FIELD_NAME}' == CHOICE {_HAS_IMAGES_YES_CHOICE})";

			return imageCondition;
		}
		
		protected Task<long> CreateCalculateImagesTotalSizeTaskAsync(IImageDataSourceSnapshotConfiguration configuration, CancellationToken token, QueryRequest queryRequest)
		{
			QueryImagesOptions options = new QueryImagesOptions
			{
				ProductionIds = configuration.ProductionIds,
				IncludeOriginalImageIfNotFoundInProductions = configuration.IncludeOriginalImageIfNotFoundInProductions
			};

			Task<long> calculateImagesTotalSizeTask = Task.Run(() => _imageFileRepository.CalculateImagesTotalSizeAsync(configuration.SourceWorkspaceArtifactId, queryRequest, options), token);
			return calculateImagesTotalSizeTask;
		}
	}
}