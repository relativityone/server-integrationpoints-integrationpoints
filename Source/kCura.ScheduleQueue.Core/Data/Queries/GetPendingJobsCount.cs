using System.Data;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class GetPendingJobsCount : IQuery<int>
	{
		private readonly IQueueDBContext _qDbContext;

		public GetPendingJobsCount(IQueueDBContext qDbContext)
		{
			_qDbContext = qDbContext;
		}

		public int Execute()
		{
			string sql = string.Format(Resources.GetPendingJobsCount, _qDbContext.TableName);
			DataTable queryResult = _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql);
			object value = queryResult.Rows[0][0];
			return (int) value;
		}
	}
}