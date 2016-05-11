using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.Core;
using Relativity.Data;
using Field = Relativity.Core.DTO.Field;

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

		public void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int sourceWorkspaceId, Relativity.Client.Choice errorStatus, string tableSuffix)
		{
		}

		public List<JobHistoryError> RetreiveJobHistoryErrors(int jobHistoryArtifactId)
		{
			return new List<JobHistoryError>();
		}
	}
}