using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetJobCustodianManagerLinks
	{
		private IDBContext _caseDBcontext;
		public GetJobCustodianManagerLinks(IDBContext caseDBcontext)
		{
			_caseDBcontext = caseDBcontext;
		}

		public DataTable Execute(string tableName, long jobID)
		{
			var sql = string.Format(Resources.Resource.GetJobCustodianManagerLinks, tableName);
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));

			return _caseDBcontext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
		}
	}
}
