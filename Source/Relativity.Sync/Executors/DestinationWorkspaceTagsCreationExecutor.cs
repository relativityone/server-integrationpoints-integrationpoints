using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagsCreationExecutor : IExecutor<IDestinationWorkspaceTagsCreationConfiguration>
	{
		private readonly IRelativitySourceCaseTagRepository _relativitySourceCaseTagRepository;
		private readonly IRelativitySourceJobTagRepository _relativitySourceJobTagRepository;
		private readonly IWorkspaceNameQuery _workspaceNameQuery;
		private readonly IJobHistoryNameQuery _jobHistoryNameQuery;
		private readonly ITagNameFormatter _tagNameFormatter;
		private readonly IFederatedInstance _federatedInstance;

		public DestinationWorkspaceTagsCreationExecutor(IRelativitySourceCaseTagRepository relativitySourceCaseTagRepository, IRelativitySourceJobTagRepository relativitySourceJobTagRepository,
			IWorkspaceNameQuery workspaceNameQuery, IJobHistoryNameQuery jobHistoryNameQuery, ITagNameFormatter tagNameFormatter, IFederatedInstance federatedInstance)
		{
			_relativitySourceCaseTagRepository = relativitySourceCaseTagRepository;
			_relativitySourceJobTagRepository = relativitySourceJobTagRepository;
			_workspaceNameQuery = workspaceNameQuery;
			_jobHistoryNameQuery = jobHistoryNameQuery;
			_tagNameFormatter = tagNameFormatter;
			_federatedInstance = federatedInstance;
		}

		public async Task ExecuteAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			RelativitySourceCaseTag sourceCaseTag = await CreateOrUpdateSourceCaseTagAsync(configuration, token).ConfigureAwait(false);
			configuration.SetSourceWorkspaceTag(sourceCaseTag.ArtifactId, sourceCaseTag.Name);

			RelativitySourceJobTag sourceJobTag = await CreateOrUpdateSourceJobTagAsync(configuration, sourceCaseTag.ArtifactId, token).ConfigureAwait(false);
			configuration.SetSourceJobTag(sourceJobTag.ArtifactId, sourceJobTag.Name);
		}

		private async Task<RelativitySourceCaseTag> CreateOrUpdateSourceCaseTagAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			string federatedInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);
			string sourceWorkspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			string sourceCaseTagName = _tagNameFormatter.CreateSourceCaseTagName(federatedInstanceName, sourceWorkspaceName, configuration.SourceWorkspaceArtifactId);

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
				sourceCaseTag = await _relativitySourceCaseTagRepository.CreateAsync(configuration.DestinationWorkspaceArtifactId, configuration.SourceWorkspaceArtifactTypeId, newSourceCaseTag).ConfigureAwait(false);
			}
			else if (!sourceCaseTag.SourceWorkspaceName.Equals(sourceWorkspaceName, StringComparison.InvariantCulture) ||
				!sourceCaseTag.SourceInstanceName.Equals(federatedInstanceName, StringComparison.InvariantCulture) ||
				!sourceCaseTag.Name.Equals(sourceCaseTagName, StringComparison.InvariantCulture))
			{
				sourceCaseTag.SourceInstanceName = federatedInstanceName;
				sourceCaseTag.SourceWorkspaceName = sourceWorkspaceName;
				sourceCaseTag.Name = sourceCaseTagName;
				await _relativitySourceCaseTagRepository.UpdateAsync(configuration.DestinationWorkspaceArtifactId, sourceCaseTag).ConfigureAwait(false);
			}

			return sourceCaseTag;
		}

		private async Task<RelativitySourceJobTag> CreateOrUpdateSourceJobTagAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, int sourceCaseTagArtifactId, CancellationToken token)
		{
			string sourceJobHistoryName = await _jobHistoryNameQuery.GetJobNameAsync(configuration.JobArtifactId, token).ConfigureAwait(false);
			string sourceJobTagName = _tagNameFormatter.FormatSourceJobTagName(sourceJobHistoryName, configuration.JobArtifactId);

			RelativitySourceJobTag sourceJobTag = await _relativitySourceJobTagRepository.ReadAsync(configuration.SourceJobArtifactTypeId, sourceCaseTagArtifactId, configuration.JobArtifactId, token)
				.ConfigureAwait(false);

			if (sourceJobTag == null)
			{
				RelativitySourceJobTag newSourceJobTag = new RelativitySourceJobTag
				{
					Name = sourceJobTagName,
					JobArtifactId = configuration.JobArtifactId,
					JobHistoryName = sourceJobHistoryName
				};
				sourceJobTag = await _relativitySourceJobTagRepository.CreateAsync(configuration.SourceJobArtifactTypeId, newSourceJobTag, token).ConfigureAwait(false);
			}
			else if (!sourceJobTag.JobHistoryName.Equals(sourceJobHistoryName, StringComparison.InvariantCulture) || !sourceJobTag.Name.Equals(sourceJobTagName, StringComparison.InvariantCulture))
			{
				sourceJobTag.JobHistoryName = sourceJobHistoryName;
				sourceJobTag.Name = sourceJobTagName;
				sourceJobTag = await _relativitySourceJobTagRepository.UpdateAsync(configuration.SourceJobArtifactTypeId, sourceJobTag, token).ConfigureAwait(false);
			}

			return sourceJobTag;
		}
	}
}
