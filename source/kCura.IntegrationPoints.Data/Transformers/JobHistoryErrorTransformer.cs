using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Transformers
{
	public class JobHistoryErrorTransformer : IDtoTransformer<JobHistoryErrorDTO, JobHistoryError>
    {
	    private readonly IRepositoryFactory _repositoryFactory;
	    private readonly int _workspaceArtifactId;

	    public JobHistoryErrorTransformer(IHelper helper, int workspaceArtifactId) 
            : this(new RepositoryFactory(helper), workspaceArtifactId)
	    {
	    }

        /// <summary>
        /// Only external usage of this constructor should be unit tests
        /// </summary>
	    internal JobHistoryErrorTransformer(IRepositoryFactory repositoryFactory, int workspaceArtifactId)
	    {
            _repositoryFactory = repositoryFactory;
            _workspaceArtifactId = workspaceArtifactId;
	    }

		/// <summary>
		/// Converts JobHistoryError object to DTO format
		/// </summary>
		/// <param name="jobHistoryError">JobHistoryError object to be transformed</param>
		/// <returns>JobHistoryError object in DTO form</returns>
		public JobHistoryErrorDTO ConvertToDto(JobHistoryError jobHistoryError)
        {
            IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(_workspaceArtifactId);
            int errorStatusChoiceArtifactId = jobHistoryError.ErrorStatus.ArtifactID;
            Guid errorStatusChoiceGuid = artifactGuidRepository.GetGuidsForArtifactIds(new List<int> { errorStatusChoiceArtifactId })[errorStatusChoiceArtifactId];
			int errorTypeChoiceArtifactId = jobHistoryError.ErrorStatus.ArtifactID;
			Guid errorTypeChoiceGuid = artifactGuidRepository.GetGuidsForArtifactIds(new List<int> { errorTypeChoiceArtifactId })[errorTypeChoiceArtifactId];

			var dto = new JobHistoryErrorDTO()
            {
                ArtifactId = jobHistoryError.ArtifactId,
				Error = jobHistoryError.Error,
				ErrorStatus = JobHistoryErrorDTO.Choices.ErrorStatus.GuidValues[errorStatusChoiceGuid],
				ErrorType = JobHistoryErrorDTO.Choices.ErrorType.GuidValues[errorTypeChoiceGuid],
				JobHistory = jobHistoryError.JobHistory,
				Name = jobHistoryError.Name,
				SourceUniqueID = jobHistoryError.SourceUniqueID,
				StackTrace = jobHistoryError.StackTrace,
				TimestampUTC = jobHistoryError.TimestampUTC
			};
            return dto;
        }

		/// <summary>
		/// Converts JobHistoryError objects to DTO format
		/// </summary>
		/// <param name="jobHistoryErrors">JobHistoryError objects to be transformed</param>
		/// <returns>List of JobHistoryError objects in DTO form</returns>
		public List<JobHistoryErrorDTO> ConvertToDto(IEnumerable<JobHistoryError> jobHistoryErrors)
        {
            return jobHistoryErrors.Select(ConvertToDto).ToList();
        }
    }
}
