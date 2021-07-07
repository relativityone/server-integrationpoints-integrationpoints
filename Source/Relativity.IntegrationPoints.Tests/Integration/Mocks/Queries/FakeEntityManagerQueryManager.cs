using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries
{
	public class FakeEntityManagerQueryManager : IEntityManagerQueryManager
	{
		private readonly RelativityInstanceTest _relativity;

		public FakeEntityManagerQueryManager(RelativityInstanceTest relativity)
		{
			_relativity = relativity;
		}

		public ICommand CreateEntityManagerResourceTable(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext, string tableName, int workspaceID)
		{
			return new ActionCommand(() =>
			{
				_relativity.EntityManagersResourceTables[tableName] = new List<EntityManagerTest>();
			});
		}

		public IQuery<DataTable> GetJobEntityManagerLinks(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext, string tableName, long jobID, int workspaceID)
		{
			List<int> managerIds = _relativity.EntityManagersResourceTables[tableName]
				.Join(
					_relativity.EntityManagersResourceTables[tableName].Where(x => x.LockedByJobId != null),
					t1 => new { t1.EntityId, t1.ManagerId },
					t2 => new { t2.EntityId, t2.ManagerId },
					(t1, t2) => new { t1.Id, t1.LockedByJobId })
				.Where(x => x.LockedByJobId == null).Select(x => x.Id)
				.ToList();

			foreach(var mgr in _relativity.EntityManagersResourceTables[tableName].Where(x => managerIds.Contains(x.Id)))
			{
				mgr.LockedByJobId = -1;
			}

			DataTable dt = DatabaseSchema.EntityManagerSchema();

			foreach(var mgr in _relativity.EntityManagersResourceTables[tableName].Where(x => x.LockedByJobId == null))
			{
				mgr.LockedByJobId = jobID;
				dt.ImportRow(mgr.AsDataRow());
			}

			return new ValueReturnQuery<DataTable>(dt);
		}

		public IQuery<DataTable> GetRelatedJobsCountWithTaskTypeExceptions(IDBContext eddsDBcontext, long rootJobID, string[] taskTypeExceptions)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn());

			int jobsCount = _relativity.JobsInQueue.Where(x =>
					(x.JobId == rootJobID || x.RootJobId == rootJobID)
					&& !taskTypeExceptions.Contains(x.TaskType))
				.Count();

			DataRow row = dt.NewRow();
			row[0] = jobsCount;
			dt.Rows.Add(row);

			return new ValueReturnQuery<DataTable>(dt);
		}

		public ICommand InsertDataToEntityManagerResourceTable(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext, string tableName, DataTable entityManagerRows, int workspaceID)
		{
			return new ActionCommand(() =>
			{
				_relativity.EntityManagersResourceTables[tableName]
					.AddRange(
						entityManagerRows.AsEnumerable().Select(x => EntityManagerTest.FromRow(x)));
			});
		}
	}
}
