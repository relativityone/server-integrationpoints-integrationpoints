using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class GetJob : IQuery<DataTable>
	{
	    private readonly IQueueDBContext _qDbContext;
	    private readonly long _jobId;

	    public GetJob(IQueueDBContext qDbContext, long jobId)
	    {
		    _qDbContext = qDbContext;
		    _jobId = jobId;
	    }

        public DataTable Execute()
		{
			string sql = string.Format(Resources.GetJobByID, _qDbContext.TableName);
		    var sqlParams = new List<SqlParameter>
		    {
		        new SqlParameter("@JobID", _jobId)
		    };

		    return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
        }
	}
}
