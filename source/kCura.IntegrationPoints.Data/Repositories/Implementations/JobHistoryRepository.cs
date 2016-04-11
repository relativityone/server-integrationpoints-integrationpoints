using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Core.Process;
using Relativity.Data;
using Field = Relativity.Core.DTO.Field;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryRepository : RelativityMassEditBase, IJobHistoryRepository
	{
		public void TagDocsWithJobHistory(int numberOfDocs, int jobHistoryInstanceArtifactId, int sourceWorkspaceId, string tableSuffix)
		{
			BaseServiceContext baseService = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(sourceWorkspaceId);

			Guid[] guids = { new Guid(DocumentMultiObjectFields.JOB_HISTORY_FIELD) };
			DataRowCollection fieldRows;
			try
			{
				fieldRows = FieldQuery.RetrieveAllByGuids(baseService.ChicagoContext.DBContext, guids).Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to query for multi-object field on Document associated with JobHistory object.", ex);
			}

			if (fieldRows.Count == 0)
			{
				throw new Exception("Multi-object field on Document associated with JobHistory object does not exist.");
			}

			Field multiObjectField = new Field(baseService, fieldRows[0]);
			try
			{
				base.TagDocumentsWithRdo(baseService, multiObjectField, numberOfDocs, jobHistoryInstanceArtifactId, Constants.TEMPORARY_DOC_TABLE_JOB_HIST + "_" + tableSuffix);
			}
			catch (Exception e)
			{
				throw new Exception("Tagging Documents with JobHistory object failed - Mass Edit failure", e);
			}
		}
	}
}
