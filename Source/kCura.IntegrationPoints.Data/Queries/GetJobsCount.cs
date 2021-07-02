using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetJobsCount : IQuery<DataTable>
	{
		private readonly IDBContext _eddsDBcontext;
		private readonly long _rootJobID;
		private readonly string[] _taskTypeExceptions;

		public GetJobsCount(IDBContext eddsDBcontext, long rootJobID, string[] taskTypeExceptions)
		{
			_eddsDBcontext = eddsDBcontext;
			_rootJobID = rootJobID;
			_taskTypeExceptions = taskTypeExceptions;
		}

		public DataTable Execute()
		{
			string sql = string.Format(Resources.Resource.GetJobsCount, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME, _rootJobID);
			if (_taskTypeExceptions != null && _taskTypeExceptions.Length > 0)
			{
				string[] taskList = _taskTypeExceptions.Select(x => string.Format("'{0}'", x)).ToArray();
				sql = sql + string.Format(" AND NOT TaskType IN ({0})", string.Join(",", taskList));
			}
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@RootJobID", _rootJobID));

			return _eddsDBcontext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
		}
	}
}
