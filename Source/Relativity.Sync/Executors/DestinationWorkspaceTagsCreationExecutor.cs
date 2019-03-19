using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagsCreationExecutor : IExecutor<IDestinationWorkspaceTagsCreationConfiguration>
	{
		private const int _DEFAULT_NAME_FIELD_LENGTH = 255;
		private const string _LOCAL_INSTANCE_NAME = "This Instance";

		private readonly ISyncLog _logger;
		private readonly IRelativitySourceCaseTagRepository _relativitySourceCaseTagRepository;
		private readonly IRelativitySourceJobTagRepository _relativitySourceJobTagRepository;
		private readonly IWorkspaceNameQuery _workspaceNameQuery;
		private readonly IJobHistoryNameQuery _jobHistoryNameQuery;

		public DestinationWorkspaceTagsCreationExecutor(IRelativitySourceCaseTagRepository relativitySourceCaseTagRepository, IRelativitySourceJobTagRepository relativitySourceJobTagRepository,
			IWorkspaceNameQuery workspaceNameQuery, IJobHistoryNameQuery jobHistoryNameQuery, ISyncLog logger)
		{
			_logger = logger;
			_relativitySourceCaseTagRepository = relativitySourceCaseTagRepository;
			_relativitySourceJobTagRepository = relativitySourceJobTagRepository;
			_workspaceNameQuery = workspaceNameQuery;
			_jobHistoryNameQuery = jobHistoryNameQuery;
		}

		public async Task ExecuteAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			string sourceWorkspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			string sourceCaseTagName = CreateSourceCaseTagName(_LOCAL_INSTANCE_NAME, sourceWorkspaceName, configuration.SourceWorkspaceArtifactId);

			RelativitySourceCaseTag sourceCaseTag = await _relativitySourceCaseTagRepository
				.ReadAsync(configuration.SourceWorkspaceArtifactTypeId, configuration.SourceWorkspaceArtifactId, _LOCAL_INSTANCE_NAME, token).ConfigureAwait(false);

			if (sourceCaseTag == null)
			{
				var newSourceCaseTag = new RelativitySourceCaseTag
				{
					SourceWorkspaceArtifactId = configuration.SourceWorkspaceArtifactId,
					SourceWorkspaceName = sourceWorkspaceName,
					SourceInstanceName = _LOCAL_INSTANCE_NAME,
					Name = sourceCaseTagName
				};
				sourceCaseTag = await _relativitySourceCaseTagRepository.CreateAsync(configuration.SourceWorkspaceArtifactTypeId, newSourceCaseTag, token).ConfigureAwait(false);
			}
			else if (!sourceCaseTag.SourceWorkspaceName.Equals(sourceWorkspaceName, StringComparison.InvariantCulture) || 
					!sourceCaseTag.SourceInstanceName.Equals(_LOCAL_INSTANCE_NAME, StringComparison.InvariantCulture) ||
					!sourceCaseTag.Name.Equals(sourceCaseTagName, StringComparison.InvariantCulture))
			{
				sourceCaseTag.SourceInstanceName = _LOCAL_INSTANCE_NAME;
				sourceCaseTag.SourceWorkspaceName = sourceWorkspaceName;
				sourceCaseTag.Name = sourceCaseTagName;
				sourceCaseTag = await _relativitySourceCaseTagRepository.UpdateAsync(configuration.SourceWorkspaceArtifactTypeId, sourceCaseTag, token).ConfigureAwait(false);
			}
			
			configuration.SetSourceWorkspaceTag(sourceCaseTag.ArtifactId, sourceCaseTagName);

			string sourceJobHistoryName = await _jobHistoryNameQuery.GetJobNameAsync(configuration.JobArtifactId, token).ConfigureAwait(false);
			string sourceJobTagName = CreateSourceJobTagName(sourceJobHistoryName, configuration.JobArtifactId);

			RelativitySourceJobTag sourceJobTag = await _relativitySourceJobTagRepository.ReadAsync(configuration.SourceJobArtifactTypeId, sourceCaseTag.ArtifactId, configuration.JobArtifactId, token)
				.ConfigureAwait(false);

			if (sourceJobTag == null)
			{
				var newSourceJobTag = new RelativitySourceJobTag
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

			configuration.SetSourceJobTag(sourceJobTag.ArtifactId, sourceJobTag.Name);
		}

		private string CreateSourceJobTagName(string jobHistoryName, int jobHistoryArtifactId)
		{
			string name = FormatSourceJobTagName(jobHistoryName, jobHistoryArtifactId);
			if (name.Length > _DEFAULT_NAME_FIELD_LENGTH)
			{
				_logger.LogWarning("Relativity Source Job Name exceeded max length and has been shortened. Full name {name}.", name);

				int overflow = name.Length - _DEFAULT_NAME_FIELD_LENGTH;
				string trimmedJobHistoryName = jobHistoryName.Substring(0, jobHistoryName.Length - overflow);
				name = FormatSourceJobTagName(trimmedJobHistoryName, jobHistoryArtifactId);
			}
			return name;
		}

		private static string FormatSourceJobTagName(string jobHistoryName, int jobHistoryArtifactId)
		{
			return $"{jobHistoryName} - {jobHistoryArtifactId}";
		}

		private string CreateSourceCaseTagName(string instanceName, string sourceWorkspaceName, int workspaceArtifactId)
		{
			string name = FormatSourceCaseTagName(instanceName, sourceWorkspaceName, workspaceArtifactId);
			if (name.Length > _DEFAULT_NAME_FIELD_LENGTH)
			{
				_logger.LogWarning("Relativity Source Case Name exceeded max length and has been shortened. Full name {name}.", name);

				int overflow = name.Length - _DEFAULT_NAME_FIELD_LENGTH;
				string trimmedInstanceName = instanceName.Substring(0, instanceName.Length - overflow);
				name = FormatSourceCaseTagName(trimmedInstanceName, sourceWorkspaceName, workspaceArtifactId);
			}
			return name;
		}

		private static string FormatSourceCaseTagName(string instanceName, string workspaceName, int workspaceArtifactId)
		{
			return $"{instanceName} - {workspaceName} - {workspaceArtifactId}";
		}
	}
}
