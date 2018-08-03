using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.EntityManager
{
	public class ManagerQueueService : IManagerQueueService
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IEddsServiceContext _eddsServiceContext;

		public ManagerQueueService(IRepositoryFactory repositoryFactory, ICaseServiceContext caseServiceContext, IEddsServiceContext eddsServiceContext)
		{
			_repositoryFactory = repositoryFactory;
			_caseServiceContext = caseServiceContext;
			_eddsServiceContext = eddsServiceContext;
		}

		public List<EntityManagerMap> GetEntityManagerLinksToProcess(Job job, Guid batchInstance, List<EntityManagerMap> entityManagerMap)
		{
			//Create temp table if does not exist and delete old links
			string tableName = GetTempTableName(job, batchInstance);
			new CreateEntityManagerResourceTable(_repositoryFactory, _caseServiceContext.SqlContext).Execute(tableName, _caseServiceContext.WorkspaceID);

			//insert job Entity Manager links
			InsertData(tableName, entityManagerMap, _caseServiceContext.WorkspaceID);

			//Get links to process
			List<EntityManagerMap> newLinkMap = GetLinksToProcess(tableName, job);

			return newLinkMap;
		}

		public bool AreAllTasksOfTheBatchDone(Job job, string[] taskTypeExceptions)
		{
			bool result = true;
			DataTable dt = new GetJobsCount(_eddsServiceContext.SqlContext).Execute(job.RootJobId ?? job.JobId, taskTypeExceptions);
			if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
			{
				int count = int.Parse(dt.Rows[0][0].ToString());
				//do not count last final job
				result = count < 2;
			}
			return result;
		}

		private List<EntityManagerMap> GetLinksToProcess(string tableName, Job job)
		{
			List<EntityManagerMap> newLinkMap = new List<EntityManagerMap>();
			DataTable dt = new GetJobEntityManagerLinks(_repositoryFactory, _caseServiceContext.SqlContext).Execute(tableName, job.JobId, _caseServiceContext.WorkspaceID);
			if (dt != null && dt.Rows != null)
			{
				foreach (DataRow row in dt.Rows)
				{
					newLinkMap.Add(new EntityManagerMap()
					{
						EntityID = row.Field<string>("EntityID"),
						OldManagerID = row.Field<string>("ManagerID")
					});
				}
			}
			return newLinkMap;
		}

		private string GetTempTableName(Job job, Guid batchInstance)
		{
			return this.GetTempTableName(job.WorkspaceID, job.RelatedObjectArtifactID, batchInstance);
		}

		private string GetTempTableName(int workspaceID, int relatedObjectArtifactID, Guid batchInstance)
		{
			// "RIP_EntityManager" is hardcoded also in corresponding SQL resource file
			return string.Format("RIP_EntityManager_{0}_{1}_{2}", workspaceID.ToString("D7"), relatedObjectArtifactID.ToString("D7"), batchInstance.ToString());
		}

		private void InsertData(string tableName, List<EntityManagerMap> entityManagerMap, int workspaceID)
		{
			IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
			DataTable dtInsertRows = GetDataTable(entityManagerMap);
			using (SqlBulkCopy sbc = new SqlBulkCopy(_caseServiceContext.SqlContext.GetConnection()))
			{
				sbc.DestinationTableName = string.Format("{0}.[{1}]", scratchTableRepository.GetResourceDBPrepend(), tableName);

				// Map the Source Column from DataTabel to the Destination Columns
				sbc.ColumnMappings.Add("EntityID", "EntityID");
				sbc.ColumnMappings.Add("ManagerID", "ManagerID");
				sbc.ColumnMappings.Add("CreatedOn", "CreatedOn");

				// Finally write to server
				sbc.WriteToServer(dtInsertRows);
				sbc.Close();
			}
		}

		private DataTable GetDataTable(List<EntityManagerMap> entityManagerMap)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add(new DataColumn("EntityID", typeof(System.String)));
			dataTable.Columns.Add(new DataColumn("ManagerID", typeof(System.String)));
			dataTable.Columns.Add(new DataColumn("CreatedOn", typeof(System.DateTime)));

			foreach (var map in entityManagerMap)
			{
				DataRow row = dataTable.NewRow();
				row["EntityID"] = map.EntityID;
				row["ManagerID"] = map.OldManagerID;
				row["CreatedOn"] = DateTime.UtcNow;
				dataTable.Rows.Add(row);
			}
			return dataTable;
		}
	}
}
