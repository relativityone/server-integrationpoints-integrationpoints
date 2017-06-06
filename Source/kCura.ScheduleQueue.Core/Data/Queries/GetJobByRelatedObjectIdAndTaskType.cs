using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetJobByRelatedObjectIdAndTaskType
    {
        private readonly IQueueDBContext _qDbContext;

        public GetJobByRelatedObjectIdAndTaskType(IQueueDBContext qDbContext)
        {
            _qDbContext = qDbContext;
        }

        public DataTable Execute(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
        {
            //Gets only scheduled job
            string sql = string.Format(Resources.GetJobByRelatedObjectIDandTaskType, _qDbContext.TableName, Utility.Array.StringArrayToCsvForSql(taskTypes.ToArray()));

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@WorkspaceID", workspaceId),
                new SqlParameter("@RelatedObjectArtifactID", relatedObjectArtifactId)
            };

            return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
        }
    }
}