using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetJobsCount
	{
		private readonly IDBContext _eddsDBcontext;
		public GetJobsCount(IDBContext eddsDBcontext)
		{
			_eddsDBcontext = eddsDBcontext;
		}

		public DataTable Execute(long rootJobID, string[] taskTypeExceptions)
		{
			var sql = string.Format(Resources.Resource.GetJobsCount, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME, rootJobID);
			if (taskTypeExceptions != null && taskTypeExceptions.Length > 0)
			{
				string[] taskList = taskTypeExceptions.Select(x => string.Format("'{0}'", x)).ToArray();
				sql = sql + string.Format(" AND NOT TaskType IN ({0})", string.Join(",", taskList));
			}
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@RootJobID", rootJobID));

			return _eddsDBcontext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
		}
	}
}
