using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.EntityManager
{
	public class ManagerQueueService : IManagerQueueService
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IEddsServiceContext _eddsServiceContext;
		private readonly IEntityManagerQueryManager _entityQueryManager;

		public ManagerQueueService(IRepositoryFactory repositoryFactory, ICaseServiceContext caseServiceContext,
			IEddsServiceContext eddsServiceContext, IEntityManagerQueryManager entityQueryManager)
		{
			_repositoryFactory = repositoryFactory;
			_caseServiceContext = caseServiceContext;
			_eddsServiceContext = eddsServiceContext;
			_entityQueryManager = entityQueryManager;
		}

		public List<EntityManagerMap> GetEntityManagerLinksToProcess(Job job, Guid batchInstance, List<EntityManagerMap> entityManagerMap)
		{
			//Create temp table if does not exist and delete old links
			string tableName = GetTempTableName(job, batchInstance);

			_entityQueryManager.CreateEntityManagerResourceTable(_repositoryFactory, _caseServiceContext.SqlContext,
					tableName, _caseServiceContext.WorkspaceID)
				.Execute();

			//insert job Entity Manager links
			InsertData(tableName, entityManagerMap, _caseServiceContext.WorkspaceID);

			//Get links to process
			List<EntityManagerMap> newLinkMap = GetLinksToProcess(tableName, job);

			return newLinkMap;
		}

		public bool AreAllTasksOfTheBatchDone(Job job, string[] taskTypeExceptions)
		{
			bool result = true;
			DataTable dt = _entityQueryManager.GetRelatedJobsCountWithTaskTypeExceptions(_eddsServiceContext.SqlContext, 
					job.RootJobId ?? job.JobId, taskTypeExceptions)
				.Execute();
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
			DataTable dt = _entityQueryManager.GetJobEntityManagerLinks(_repositoryFactory,
					_caseServiceContext.SqlContext, tableName, job.JobId, _caseServiceContext.WorkspaceID)
				.Execute();

			List<EntityManagerMap> newLinkMap = new List<EntityManagerMap>();
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
			DataTable dtInsertRows = GetDataTable(entityManagerMap);

			_entityQueryManager.InsertDataToEntityManagerResourceTable(_repositoryFactory, _caseServiceContext.SqlContext, tableName,
					dtInsertRows, _caseServiceContext.WorkspaceID)
				.Execute();
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
