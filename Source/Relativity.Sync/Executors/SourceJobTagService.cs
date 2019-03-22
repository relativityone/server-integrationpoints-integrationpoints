using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class SourceJobTagService : ISourceJobTagService
	{
		private readonly IRelativitySourceJobTagRepository _relativitySourceJobTagRepository;
		private readonly IJobHistoryNameQuery _jobHistoryNameQuery;
		private readonly ITagNameFormatter _tagNameFormatter;

		public SourceJobTagService(IRelativitySourceJobTagRepository relativitySourceJobTagRepository, IJobHistoryNameQuery jobHistoryNameQuery, ITagNameFormatter tagNameFormatter)
		{
			_relativitySourceJobTagRepository = relativitySourceJobTagRepository;
			_jobHistoryNameQuery = jobHistoryNameQuery;
			_tagNameFormatter = tagNameFormatter;
		}

		public async Task<RelativitySourceJobTag> CreateSourceJobTagAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, int sourceCaseTagArtifactId, CancellationToken token)
		{
			string sourceJobHistoryName = await _jobHistoryNameQuery.GetJobNameAsync(configuration.JobArtifactId, token).ConfigureAwait(false);
			string sourceJobTagName = _tagNameFormatter.FormatSourceJobTagName(sourceJobHistoryName, configuration.JobArtifactId);

			RelativitySourceJobTag sourceJobTag = new RelativitySourceJobTag
			{
				Name = sourceJobTagName,
				ArtifactTypeId = configuration.SourceJobArtifactTypeId,
				JobHistoryArtifactId = configuration.JobArtifactId,
				JobHistoryName = sourceJobHistoryName,
				SourceCaseTagArtifactId = sourceCaseTagArtifactId
			};

			RelativitySourceJobTag newSourceJobTag = await _relativitySourceJobTagRepository.CreateAsync(configuration.DestinationWorkspaceArtifactId, sourceJobTag, token).ConfigureAwait(false);

			return newSourceJobTag;
		}
	}
}