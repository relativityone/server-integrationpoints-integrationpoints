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
			BaseServiceContext baseService = claimsPrincipal.GetUnversionContext(sourceWorkspaceId);

			Guid[] guids = { new Guid(JobHistoryErrorFieldGuids.ErrorStatus) };
			DataRowCollection fieldRows;
			try
			{
				fieldRows = FieldQuery.RetrieveAllByGuids(baseService.ChicagoContext.DBContext, guids).Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_ERRORS_SO_QUERY_ERROR, ex);
			}

			if (fieldRows.Count == 0)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_ERRORS_SO_EXISTENCE_ERROR);
			}

			Field singleObjectField = new Field(baseService, fieldRows[0]);
			string fullTableName = $"{Constants.TEMPORARY_DOC_TABLE_JOB_HIST}_{tableSuffix}";
			try
			{
				//base.MassEditField(baseService, singleObjectField, numberOfDocs, jobHistoryInstanceArtifactId, fullTableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_MASS_EDIT_FAILURE, e);
			}
		}

		public List<JobHistoryError> RetreiveJobHistoryErrors(int jobHistoryArtifactId)
		{
			return new List<JobHistoryError>();
		}
	}
}