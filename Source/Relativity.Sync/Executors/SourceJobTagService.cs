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

        public async Task<RelativitySourceJobTag> CreateOrReadSourceJobTagAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, int sourceCaseTagArtifactId, CancellationToken token)
        {
            RelativitySourceJobTag sourceJobTag = await _relativitySourceJobTagRepository.ReadAsync(configuration.DestinationWorkspaceArtifactId, configuration.JobHistoryArtifactId, token).ConfigureAwait(false);
            if (sourceJobTag != null)
            {
                return sourceJobTag;
            }

            string sourceJobHistoryName = await _jobHistoryNameQuery.GetJobNameAsync(configuration.JobHistoryObjectTypeGuid, configuration.JobHistoryArtifactId, configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
            string sourceJobTagName = _tagNameFormatter.FormatSourceJobTagName(sourceJobHistoryName, configuration.JobHistoryArtifactId);

            RelativitySourceJobTag sourceJobTagToCreate = new RelativitySourceJobTag
            {
                Name = sourceJobTagName,
                JobHistoryArtifactId = configuration.JobHistoryArtifactId,
                JobHistoryName = sourceJobHistoryName,
                SourceCaseTagArtifactId = sourceCaseTagArtifactId
            };

            RelativitySourceJobTag newSourceJobTag = await _relativitySourceJobTagRepository.CreateAsync(configuration.DestinationWorkspaceArtifactId, sourceJobTagToCreate, token).ConfigureAwait(false);

            return newSourceJobTag;
        }
    }
}
