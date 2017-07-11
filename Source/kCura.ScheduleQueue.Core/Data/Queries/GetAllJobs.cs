using System.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class GetAllJobs
	{
		private readonly IQueueDBContext _qDbContext;

		public GetAllJobs(IQueueDBContext qDbContext)
		{
			_qDbContext = qDbContext;
		}

		public DataTable Execute()
		{
			string sql = string.Format(Resources.GetAllJobs, _qDbContext.TableName);
			return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql);
		}
	}
}