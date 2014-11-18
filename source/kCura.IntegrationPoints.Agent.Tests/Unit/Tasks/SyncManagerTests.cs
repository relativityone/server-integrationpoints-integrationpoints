using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	[TestFixture]
	public class SyncManagerTests
	{
		[Test]
		public void CreateJobs_JobHasMoreThanBatchSize_CreatesTwoBatches()
		{
			//ARRANGE
			var factory = NSubstitute.Substitute.For<IDataProviderFactory>();
			var jobManager = NSubstitute.Substitute.For<IJobManager>();
			var helper = NSubstitute.Substitute.For<IntegrationPointHelper>();
			SyncManager manager = new SyncManager(factory, jobManager, helper);

			var reader = NSubstitute.Substitute.For<IDataReader>();
			reader.Read().Returns(x => true, x => true, x => true, x => false);
			var batchSize = 2;

			//ACT
			manager.CreateJobs(reader, batchSize);
			

			//ASSERT
			jobManager.Received(2).CreateJob(Arg.Any<IEnumerable<string>>(), TaskType.SyncWorker);
		}

		[Test]
		public void CreateJobs_JobHasMoreThanBatchSize_CreatesCorrectBatchSizes()
		{
			//ARRANGE
			var factory = NSubstitute.Substitute.For<IDataProviderFactory>();
			var jobManager = NSubstitute.Substitute.For<IJobManager>();
			var helper = NSubstitute.Substitute.For<IntegrationPointHelper>();
			SyncManager manager = new SyncManager(factory, jobManager, helper);

			var reader = NSubstitute.Substitute.For<IDataReader>();
			reader.Read().Returns(x => true, x => true,	x=>true, x=>false);
			var batchSize = 2;

			//ACT
			manager.CreateJobs(reader, batchSize);


			//ASSERT
			jobManager.Received().CreateJob(Arg.Is<List<string>>(x=>x.Count == batchSize), Arg.Any<TaskType>());
			
		}

		[Test]
		public void CreateJobs_JobHasBatchSize_CreatesOnlyOneBatch()
		{
			//ARRANGE
			var factory = NSubstitute.Substitute.For<IDataProviderFactory>();
			var jobManager = NSubstitute.Substitute.For<IJobManager>();
			var helper = NSubstitute.Substitute.For<IntegrationPointHelper>();
			SyncManager manager = new SyncManager(factory, jobManager, helper);

			var reader = NSubstitute.Substitute.For<IDataReader>();
			reader.Read().Returns(x => true, x => true, x => false);
			var batchSize = 2;

			//ACT
			manager.CreateJobs(reader, batchSize);


			//ASSERT
			jobManager.Received(1).CreateJob(Arg.Any<List<string>>(), Arg.Any<TaskType>());

		}

		[Test]
		public void CreateJobs_JobHasLessThanBatchSize_CreatesOnlyOneBatch()
		{
			//ARRANGE
			var factory = NSubstitute.Substitute.For<IDataProviderFactory>();
			var jobManager = NSubstitute.Substitute.For<IJobManager>();
			var helper = NSubstitute.Substitute.For<IntegrationPointHelper>();
			SyncManager manager = new SyncManager(factory, jobManager, helper);

			var reader = NSubstitute.Substitute.For<IDataReader>();
			reader.Read().Returns(x => true, x => true, x => false);
			var batchSize = 3;

			//ACT
			manager.CreateJobs(reader, batchSize);


			//ASSERT
			jobManager.Received(1).CreateJob(Arg.Any<List<string>>(), Arg.Any<TaskType>());

		}
		


	}
}
