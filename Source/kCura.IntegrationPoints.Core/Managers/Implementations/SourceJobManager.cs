using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Utils;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class SourceJobManager : ISourceJobManager
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IAPILog _logger;

        public SourceJobManager(IRepositoryFactory repositoryFactory, IHelper helper)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<SourceJobManager>();
            _repositoryFactory = repositoryFactory;
        }

        public SourceJobDTO CreateSourceJobDto(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int jobHistoryArtifactId, int sourceWorkspaceRdoInstanceArtifactId)
        {
            ISourceJobRepository sourceJobRepository = _repositoryFactory.GetSourceJobRepository(destinationWorkspaceArtifactId);

            string jobHistoryName = GetJobHistoryName(sourceWorkspaceArtifactId, jobHistoryArtifactId);
            var jobHistoryDto = new SourceJobDTO
            {
                Name = GenerateSourceJobName(jobHistoryName, jobHistoryArtifactId),
                SourceWorkspaceArtifactId = sourceWorkspaceRdoInstanceArtifactId,
                JobHistoryArtifactId = jobHistoryArtifactId,
                JobHistoryName = jobHistoryName
            };

            int artifactId = sourceJobRepository.Create(jobHistoryDto);
            jobHistoryDto.ArtifactId = artifactId;

            return jobHistoryDto;
        }

        private string GetJobHistoryName(int sourceWorkspaceArtifactId, int jobHistoryArtifactId)
        {
            IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(sourceWorkspaceArtifactId);

            string jobHistoryName = jobHistoryRepository.GetJobHistoryName(jobHistoryArtifactId);
            return jobHistoryName;
        }

        private string GenerateSourceJobName(string jobHistoryName, int jobHistoryArtifactId)
        {
            string name = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(jobHistoryName, jobHistoryArtifactId);
            if (name.Length > Data.Constants.DEFAULT_NAME_FIELD_LENGTH)
            {
                _logger.LogWarning("Relativity Source Job Name length {nameLength} exceeded max length and has been shortened. Job history artifact id: {jobHistoryArtifactId}.", name.Length, jobHistoryArtifactId);

                int overflow = name.Length - Data.Constants.DEFAULT_NAME_FIELD_LENGTH;
                string trimmedJobHistoryName = jobHistoryName.Substring(0, jobHistoryName.Length - overflow);
                name = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(trimmedJobHistoryName, jobHistoryArtifactId);
            }
            return name;
        }
    }
}