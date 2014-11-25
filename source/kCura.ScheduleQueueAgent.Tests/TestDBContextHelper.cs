using kCura.Data.RowDataGateway;
using Relativity.API;

namespace kCura.ScheduleQueueAgent.Tests
{
	public class TestDBContextHelper
	{
		public IDBContext GetEDDSDBContext()
		{
			return new Relativity.API.DBContext(new Context(kCura.Data.RowDataGateway.Config.ConnectionString));
		}
	}
}
