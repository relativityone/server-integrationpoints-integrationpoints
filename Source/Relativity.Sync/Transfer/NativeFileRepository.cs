using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Toggles;
using Relativity.Toggles;

namespace Relativity.Sync.Transfer
{
    internal class NativeFileRepository : INativeFileRepository
    {
        private readonly INativeFileRepository _nativeFileRepository;

        public NativeFileRepository(IToggleProvider toggleProvider, ISearchManagerFactory searchManagerFactory, ISourceServiceFactoryForUser serviceFactoryForUser, IAPILog logger, SyncJobParameters parameters)
        {
            if (toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>())
            {
                _nativeFileRepository = new NativeFileRepositoryKepler(serviceFactoryForUser, logger, parameters);
            }
            else
            {
                _nativeFileRepository = new NativeFileRepositoryWebAPI(searchManagerFactory, logger);
            }
        }

        public async Task<IEnumerable<INativeFile>> QueryAsync(int workspaceId, ICollection<int> documentIds)
        {
            return await _nativeFileRepository.QueryAsync(workspaceId, documentIds).ConfigureAwait(false);
        }
    }
}
