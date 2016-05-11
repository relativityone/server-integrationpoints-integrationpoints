using System;
using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Utility.Extensions;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryErrorRepository : RelativityMassEditBase, IJobHistoryErrorRepository
	{
		private readonly IGenericLibrary<JobHistoryError> _jobHistoryErrorLibrary;
		private readonly IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> _dtoTransformer;

		public JobHistoryErrorRepository(IHelper helper, int workspaceArtifactId)
			: this(new RsapiClientLibrary<JobHistoryError>(helper, workspaceArtifactId), 
                  new JobHistoryErrorTransformer(helper, workspaceArtifactId))
		{
		}

		/// <summary>
		/// To be used externally by unit tests only
		/// </summary>
		internal JobHistoryErrorRepository(IGenericLibrary<JobHistoryError> jobHistoryErrorLibrary, IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> dtoTransformer)
		{
			_jobHistoryErrorLibrary = jobHistoryErrorLibrary;
			_dtoTransformer = dtoTransformer;
		}

		public List<JobHistoryError> RetreiveJobHistoryErrors(int jobHistoryArtifactId)
		{
			List<JobHistoryError> jobHistoryErrors = null;
			var query = new Query<RDO>();

			try
			{
				query.ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.JobHistoryError);
				query.Condition = new TextCondition(Guid.Parse(JobHistoryErrorFieldGuids.JobHistory), TextConditionEnum.EqualTo, jobHistoryArtifactId.ToString());
				query.Fields = FieldValue.AllFields;

				jobHistoryErrors = _jobHistoryErrorLibrary.Query(query);
			}
			catch (Exception ex)
			{
				throw new Exception(System.String.Format(JobHistoryErrorErrors.JOB_HISTORY_ERROR_RETRIEVE_FAILURE, jobHistoryArtifactId), ex);
			}

			if (jobHistoryErrors.IsNullOrEmpty())
			{
				throw new Exception(System.String.Format(JobHistoryErrorErrors.JOB_HISTORY_ERROR_RETRIEVE_NO_RESULTS, jobHistoryArtifactId));
			}

			return jobHistoryErrors;
		}

		public void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int sourceWorkspaceId, Relativity.Client.Choice errorStatus, string tableSuffix)
		{
		}
	}
}