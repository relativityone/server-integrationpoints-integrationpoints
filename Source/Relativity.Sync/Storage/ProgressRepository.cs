using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class ProgressRepository : IProgressRepository
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
		private readonly ISyncLog _logger;

		public ProgressRepository(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, ISyncLog logger)
		{
			_serviceFactoryForAdmin = serviceFactoryForAdmin;
			_logger = logger;
		}

		public Task<IProgress> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, SyncJobStatus status)
		{
			var createProgressDto = new CreateProgressDto(name, order, status, syncConfigurationArtifactId, workspaceArtifactId);
			return Progress.CreateAsync(_serviceFactoryForAdmin, _logger, createProgressDto);
		}

		public Task<IProgress> GetAsync(int workspaceArtifactId, int artifactId)
		{
			return Progress.GetAsync(_serviceFactoryForAdmin, _logger, workspaceArtifactId, artifactId);
		}

		public Task<IReadOnlyCollection<IProgress>> QueryAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			return Progress.QueryAllAsync(_serviceFactoryForAdmin, _logger, workspaceArtifactId, syncConfigurationArtifactId);
		}

		public Task<IProgress> QueryAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name)
		{
			return Progress.QueryAsync(_serviceFactoryForAdmin, _logger, workspaceArtifactId, syncConfigurationArtifactId, name);
		}
	}
}