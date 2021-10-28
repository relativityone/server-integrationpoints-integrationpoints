using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Toggles;
using Relativity.Toggles;

namespace Relativity.Sync.Transfer
{
	internal class ImageFileRepository : IImageFileRepository
    {
        private readonly IImageFileRepository _imageFileRepository;

        public ImageFileRepository(IToggleProvider toggleProvider, ISearchManagerFactory searchManagerFactory, ISourceServiceFactoryForUser serviceFactory, ISyncLog logger, SyncJobParameters parameters)
        {
            if (toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>())
            {
                _imageFileRepository = new ImageFileRepositoryKepler(serviceFactory, logger, parameters);
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

