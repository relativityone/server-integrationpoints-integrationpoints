using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Toggles;
using Relativity.Toggles;

namespace Relativity.Sync.Transfer
{
	internal class ImageFileRepository : IImageFileRepository
    {
        private readonly IImageFileRepository _imageFileRepository;

        public ImageFileRepository(IToggleProvider toggleProvider, ISearchManagerFactory searchManagerFactory, ISourceServiceFactoryForUser _serviceFactoryForUser, IAPILog logger, SyncJobParameters parameters)
        {
            if (toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>())
            {
                _imageFileRepository = new ImageFileRepositoryKepler(_serviceFactoryForUser, logger, parameters);
            }
            else
            {
                _imageFileRepository = new ImageFileRepositoryWebAPI(searchManagerFactory, logger);
            }
        }

        public async Task<IEnumerable<ImageFile>> QueryImagesForDocumentsAsync(int workspaceId, int[] documentIds, QueryImagesOptions options)
        {
            return await _imageFileRepository.QueryImagesForDocumentsAsync(workspaceId, documentIds, options);
        }
    }
}

