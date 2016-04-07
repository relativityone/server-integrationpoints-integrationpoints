using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Core.Process;
using Relativity.Data;
using Field = Relativity.Core.DTO.Field;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryRepository : IJobHistoryRepository
	{
		private static int BATCH_SIZE = 1000;

		public void TagDocsWithJobHistory(int numberOfDocs, int jobHistoryInstanceArtifactId, int sourceWorkspaceId, string tableSuffix)
		{
			BaseServiceContext baseService = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(sourceWorkspaceId);

			Guid[] guids = { new Guid(DocumentMultiObjectFields.JOB_HISTORY_FIELD)};
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
			multiObjectField.Value = GetMultiObjectListUpdate(jobHistoryInstanceArtifactId);
			var document = new global::Relativity.Query.ArtifactType(10, "Document");

			MassProcessHelper.MassProcessInitArgs initArgs = new MassProcessHelper.MassProcessInitArgs(Constants.TEMPORARY_DOC_TABLE_JOB_HIST + "_" + tableSuffix, numberOfDocs, false);
			SqlMassProcessBatch batch = new SqlMassProcessBatch(baseService, initArgs, BATCH_SIZE);

			Field[] fields =
			{
				multiObjectField
			};

			Edit massEdit = new Edit(baseService, batch, fields, BATCH_SIZE, String.Empty, true, true, false, document);
			try
			{
				massEdit.Execute(true);
			}
			catch (Exception e)
			{
				throw new Exception("Tagging Documents with JobHistory object failed - Mass Edit failure", e);
			}
		}

		internal MultiObjectListUpdate GetMultiObjectListUpdate(int destinationWorkspaceInstanceId)
		{
			var objectstoUpdate = new MultiObjectListUpdate();
			var instances = new List<int>()
			{
				destinationWorkspaceInstanceId
			};

			objectstoUpdate.tristate = true;
			objectstoUpdate.Selected = instances;

			return objectstoUpdate;
		}
	}
}
