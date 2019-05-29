using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class NativeFileRepository : INativeFileRepository
	{
		private readonly ISourceServiceFactoryForUser _serviceFactory;

		public NativeFileRepository(ISourceServiceFactoryForUser serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<IEnumerable<INativeFile>> QueryAsync(int workspaceId, ICollection<int> documentIds)
		{
			return await NativeFile.QueryAsync(_serviceFactory, workspaceId, documentIds).ConfigureAwait(false);
		}
	}
}
