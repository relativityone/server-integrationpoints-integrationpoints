using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetJobEntityManagerLinks
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDBContext _caseDBcontext;

		public GetJobEntityManagerLinks(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext)
		{
			_repositoryFactory = repositoryFactory;
			_caseDBcontext = caseDBcontext;
		}

		public DataTable Execute(string tableName, long jobID, int workspaceID)
		{
			IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
			var sql = string.Format(Resources.Resource.GetJobEntityManagerLinks, scratchTableRepository.GetResourceDBPrepend(), tableName);
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));

			return _caseDBcontext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
		}
	}
}
