using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetJobEntityManagerLinks : IQuery<DataTable>
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDBContext _caseDBcontext;
		private readonly string _tableName;
		private readonly long _jobID;
		private readonly int _workspaceID;

		public GetJobEntityManagerLinks(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext,
			string tableName, long jobID, int workspaceID)
		{
			_repositoryFactory = repositoryFactory;
			_caseDBcontext = caseDBcontext;
			
			_tableName = tableName;
			_jobID = jobID;
			_workspaceID = workspaceID;
		}

		public DataTable Execute()
		{
			IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(_workspaceID, string.Empty, string.Empty);
			var sql = string.Format(Resources.Resource.GetJobEntityManagerLinks, scratchTableRepository.GetResourceDBPrepend(), _tableName);
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", _jobID));

			return _caseDBcontext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
		}
	}
}
