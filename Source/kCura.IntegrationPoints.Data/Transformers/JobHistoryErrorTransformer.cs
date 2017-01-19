using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json.Linq;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Transformers
{
	public class JobHistoryErrorTransformer : IDtoTransformer<JobHistoryErrorDTO, JobHistoryError>
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly int _workspaceArtifactId;

		public JobHistoryErrorTransformer(IHelper helper, int workspaceArtifactId)
			: this(new RepositoryFactory(helper, helper.GetServicesManager()), workspaceArtifactId)
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
			Guid errorStatusChoiceGuid = artifactGuidRepository.GetGuidsForArtifactIds(new List<int> { jobHistoryError.ErrorStatus.ArtifactID })[jobHistoryError.ErrorStatus.ArtifactID];
			Guid errorTypeChoiceGuid = artifactGuidRepository.GetGuidsForArtifactIds(new List<int> { jobHistoryError.ErrorType.ArtifactID })[jobHistoryError.ErrorType.ArtifactID];

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

		/// <summary>
		/// Converts ArtifactDTO object to JobHistoryErrorDTO format
		/// </summary>
		/// <param name="jobHistoryError">ArtifactDTO object to be transformed</param>
		/// <returns>List of JobHistoryError objects in DTO form</returns>
		public JobHistoryErrorDTO ConvertArtifactDtoToDto(ArtifactDTO jobHistoryError)
		{
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(_workspaceArtifactId);
			IDictionary<Guid, int> tempMappingDict = artifactGuidRepository.GetArtifactIdsForGuids(JobHistoryErrorDTO.Choices.ErrorStatus.GuidList);
			IDictionary<int, JobHistoryErrorDTO.Choices.ErrorStatus.Values> errorStatusChoicesMapping = new Dictionary<int, JobHistoryErrorDTO.Choices.ErrorStatus.Values>();
			foreach (Guid guid in JobHistoryErrorDTO.Choices.ErrorStatus.GuidList)
			{
				errorStatusChoicesMapping.Add(tempMappingDict[guid], JobHistoryErrorDTO.Choices.ErrorStatus.GuidValues[guid]);
			}
			tempMappingDict = artifactGuidRepository.GetArtifactIdsForGuids(JobHistoryErrorDTO.Choices.ErrorType.GuidList);
			IDictionary<int, JobHistoryErrorDTO.Choices.ErrorType.Values> errorTypeChoicesMapping = new Dictionary<int, JobHistoryErrorDTO.Choices.ErrorType.Values>();
			foreach (Guid guid in JobHistoryErrorDTO.Choices.ErrorType.GuidList)
			{
				errorTypeChoicesMapping.Add(tempMappingDict[guid], JobHistoryErrorDTO.Choices.ErrorType.GuidValues[guid]);
			}

			IDictionary<string, ArtifactFieldDTO> fieldMapping = jobHistoryError.Fields.ToDictionary(k => k.Name, v => v);

			var jobHistoryErrorDTO = new JobHistoryErrorDTO()
			{
				ArtifactId = jobHistoryError.ArtifactId,
				Error = (string)fieldMapping[JobHistoryErrorDTO.FieldNames.Error].Value,
				ErrorStatus = errorStatusChoicesMapping[(int)JObject.Parse(fieldMapping[JobHistoryErrorDTO.FieldNames.ErrorStatus].Value.ToString()).GetValue("ArtifactID")],
				ErrorType = errorTypeChoicesMapping[(int)JObject.Parse(fieldMapping[JobHistoryErrorDTO.FieldNames.ErrorType].Value.ToString()).GetValue("ArtifactID")],
				JobHistory = (int?)JObject.Parse(fieldMapping[JobHistoryErrorDTO.FieldNames.JobHistory].Value.ToString()).GetValue("ArtifactID"),
				Name = (string)fieldMapping["Name"].Value,
				SourceUniqueID = (string)fieldMapping[JobHistoryErrorDTO.FieldNames.SourceUniqueID].Value,
				StackTrace = (string)fieldMapping[JobHistoryErrorDTO.FieldNames.StackTrace].Value,
				TimestampUTC = Convert.ToDateTime(fieldMapping[JobHistoryErrorDTO.FieldNames.TimestampUTC].Value)
			};

			return jobHistoryErrorDTO;
		}

		/// <summary>
		/// Converts ArtifactDTO objects to JobHistoryErrorDTO format
		/// </summary>
		/// <param name="jobHistoryErrors">ArtifactDTO objects to be transformed</param>
		/// <returns>List of JobHistoryError objects in DTO form</returns>
		public List<JobHistoryErrorDTO> ConvertArtifactDtoToDto(IEnumerable<ArtifactDTO> jobHistoryErrors)
		{
			return jobHistoryErrors.Select(ConvertArtifactDtoToDto).ToList();
		}
	}
}