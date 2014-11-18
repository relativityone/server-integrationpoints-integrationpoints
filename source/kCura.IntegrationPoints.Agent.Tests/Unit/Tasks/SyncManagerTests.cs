using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	[TestFixture]
	public class SyncManagerTests
	{
		[Test]
		public void Execute_JobHasMoreThanBatchSize_CreatesTwoBatches()
		{
			//ARRANGE
			var factory = NSubstitute.Substitute.For<IDataProviderFactory>();
			var jobManager = NSubstitute.Substitute.For<IJobManager>();
			var helper = NSubstitute.Substitute.For<IntegrationPointHelper>();
			var manager = new SyncManager(factory, jobManager, helper);

			//ACT

			//ASSERT

		}
	}
}
