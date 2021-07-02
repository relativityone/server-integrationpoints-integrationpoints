using kCura.IntegrationPoints.Core.Services.EntityManager;
using kCura.ScheduleQueue.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeManagerQueueService : IManagerQueueService
	{
		public bool AreAllTasksOfTheBatchDone(Job job, string[] taskTypeExceptions)
		{
			return true;
		}

		public List<EntityManagerMap> GetEntityManagerLinksToProcess(Job job, Guid batchInstance, List<EntityManagerMap> entityManagerMap)
		{
			throw new NotImplementedException();
		}
	}
}
