using System;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Core;
using Relativity.Data;
using Field = Relativity.Core.DTO.Field;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryRepository : RelativityMassEditBase, IJobHistoryRepository
	{
		public void TagDocsWithJobHistory(ClaimsPrincipal claimsPrincipal, int numberOfDocs, int jobHistoryInstanceArtifactId, int sourceWorkspaceId, string tableSuffix)
		{
			if (numberOfDocs <= 0)
			{
				return;
			}

			BaseServiceContext baseService = claimsPrincipal.GetUnversionContext(sourceWorkspaceId);

			Guid[] guids = { new Guid(DocumentMultiObjectFields.JOB_HISTORY_FIELD) };
			DataRowCollection fieldRows;
			try
			{
				fieldRows = FieldQuery.RetrieveAllByGuids(baseService.ChicagoContext.DBContext, guids).Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_MO_QUERY_ERROR, ex);
			}

			if (fieldRows.Count == 0)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_MO_EXISTENCE_ERROR);
			}

			Field multiObjectField = new Field(baseService, fieldRows[0]);
			string fullTableName = $"{Constants.TEMPORARY_DOC_TABLE_JOB_HIST}_{tableSuffix}";
			try
			{
				base.TagDocumentsWithRdo(baseService, multiObjectField, numberOfDocs, jobHistoryInstanceArtifactId, fullTableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_MASS_EDIT_FAILURE, e);
			}
		}
	}
}