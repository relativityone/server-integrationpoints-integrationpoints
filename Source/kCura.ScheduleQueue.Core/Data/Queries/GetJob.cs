using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class GetJob
	{
	    private readonly IQueueDBContext _qDbContext;

	    public GetJob(IQueueDBContext qDbContext)
	    {
	        _qDbContext = qDbContext;
	    }

        public DataTable Execute(long jobId)
		{
			string sql = string.Format(Resources.GetJobByID, _qDbContext.TableName);
		    var sqlParams = new List<SqlParameter>
		    {
		        new SqlParameter("@JobID", jobId)
		    };

		    return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
        }
	}
}
