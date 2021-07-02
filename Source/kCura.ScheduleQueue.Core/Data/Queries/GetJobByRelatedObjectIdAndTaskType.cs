using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetJobByRelatedObjectIdAndTaskType : IQuery<DataTable>
    {
        private readonly IQueueDBContext _qDbContext;
        private readonly int _workspaceId;
        private readonly int _relatedObjectArtifactId;
        private readonly List<string> _taskTypes;

        public GetJobByRelatedObjectIdAndTaskType(IQueueDBContext qDbContext, int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
        {
	        _qDbContext = qDbContext;
	        _workspaceId = workspaceId;
	        _relatedObjectArtifactId = relatedObjectArtifactId;
	        _taskTypes = taskTypes;
        }

        public DataTable Execute()
        {
            //Gets only scheduled job
            string sql = string.Format(Resources.GetJobByRelatedObjectIDandTaskType, _qDbContext.TableName, Utility.Array.StringArrayToCsvForSql(_taskTypes.ToArray()));

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@WorkspaceID", _workspaceId),
                new SqlParameter("@RelatedObjectArtifactID", _relatedObjectArtifactId)
            };

            return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
        }
    }
}