using kCura.ScheduleQueueAgent.Services;
using NUnit.Framework;
using Relativity.API;

namespace kCura.ScheduleQueueAgent.Tests.Integration.Services
{
	[TestFixture]
	public class JobServiceTests
	{
		[Test]
		[Explicit]
		public void Run_Agent()
		{
			IDBContext dbContext = null;
			var jobService = new JobService(dbContext);
		}
	}
}
