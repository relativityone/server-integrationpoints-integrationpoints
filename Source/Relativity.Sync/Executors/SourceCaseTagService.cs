using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class SourceCaseTagService : ISourceCaseTagService
	{
		private readonly IRelativitySourceCaseTagRepository _relativitySourceCaseTagRepository;
		private readonly IWorkspaceNameQuery _workspaceNameQuery;
		private readonly IFederatedInstance _federatedInstance;
		private readonly ITagNameFormatter _tagNameFormatter;
		private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;

		public SourceCaseTagService(IRelativitySourceCaseTagRepository relativitySourceCaseTagRepository, IWorkspaceNameQuery workspaceNameQuery,
			IFederatedInstance federatedInstance, ITagNameFormatter tagNameFormatter, ISourceServiceFactoryForUser serviceFactoryForUser)
		{
			_relativitySourceCaseTagRepository = relativitySourceCaseTagRepository;
			_workspaceNameQuery = workspaceNameQuery;
			_federatedInstance = federatedInstance;
			_tagNameFormatter = tagNameFormatter;
			_serviceFactoryForUser = serviceFactoryForUser;
		}

		public async Task<RelativitySourceCaseTag> CreateOrUpdateSourceCaseTagAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			string federatedInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);
			string sourceWorkspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(_serviceFactoryForUser, configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			string sourceCaseTagName = _tagNameFormatter.FormatSourceCaseTagName(federatedInstanceName, sourceWorkspaceName, configuration.SourceWorkspaceArtifactId);

			RelativitySourceCaseTag sourceCaseTag = await _relativitySourceCaseTagRepository
				.ReadAsync(configuration.DestinationWorkspaceArtifactId, configuration.SourceWorkspaceArtifactId, federatedInstanceName, token).ConfigureAwait(false);

			if (sourceCaseTag == null)
			{
				RelativitySourceCaseTag newSourceCaseTag = new RelativitySourceCaseTag
				{
					SourceWorkspaceArtifactId = configuration.SourceWorkspaceArtifactId,
					SourceWorkspaceName = sourceWorkspaceName,
					SourceInstanceName = federatedInstanceName,
					Name = sourceCaseTagName
				};
				sourceCaseTag = await _relativitySourceCaseTagRepository.CreateAsync(configuration.DestinationWorkspaceArtifactId, newSourceCaseTag).ConfigureAwait(false);
			}
			else if (sourceCaseTag.RequiresUpdate(sourceCaseTagName, federatedInstanceName, sourceWorkspaceName))
			{
				sourceCaseTag.SourceInstanceName = federatedInstanceName;
				sourceCaseTag.SourceWorkspaceName = sourceWorkspaceName;
				sourceCaseTag.Name = sourceCaseTagName;
				await _relativitySourceCaseTagRepository.UpdateAsync(configuration.DestinationWorkspaceArtifactId, sourceCaseTag).ConfigureAwait(false);
			}

			return sourceCaseTag;
		}
	}
}