using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class GetJob
	{
		private readonly IQueueDBContext _qDbContext = null;

		public GetJob(IQueueDBContext qDbContext)
		{
			this._qDbContext = qDbContext;
		}

		public DataRow Execute(long jobId)
		{
			string sql = string.Format(Resources.GetJobByID, _qDbContext.TableName);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobId));

			return ExecuteList(sql, sqlParams)?[0];
		}

		public List<DataRow> Execute(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
		{
			//Gets only scheduled job
			string sql = string.Format(Resources.GetJobByRelatedObjectIDandTaskType, _qDbContext.TableName, Utility.Array.StringArrayToCsvForSql(taskTypes.ToArray()));

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@WorkspaceID", workspaceId),
				new SqlParameter("@RelatedObjectArtifactID", relatedObjectArtifactId)
			};

			return ExecuteList(sql, sqlParams);
		}

		private List<DataRow> ExecuteList(string sql, List<SqlParameter> sqlParams)
		{
			using (DataTable dataTable = _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray()))
			{
				if (dataTable?.Rows != null && dataTable.Rows.Count > 0)
				{
					return dataTable.Rows.Cast<DataRow>().ToList();
				}
				return null;
			}
		}
	}
}
