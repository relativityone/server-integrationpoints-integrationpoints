using System;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Tests;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	[TestFixture]
	public class SyncManagerTests
	{

		#region GetBatchInstance
		private Guid defaultGuidValue = new Guid("4258D67D-63D4-4902-A48A-B1B19649ABFA");
		private Guid jobGuidValue = new Guid("0D01AF2F-5AF5-4F4D-820C-90471AD75750");

		[Test]
		public void GetBatchInstance_NoJobDetails_CorrectOutput()
		{
			//ARRANGE
			var serializer = NSubstitute.Substitute.For<kCura.Apps.Common.Utils.Serializers.JSONSerializer>();
			var guidService = NSubstitute.Substitute.For<IGuidService>();
			guidService.NewGuid().Returns(defaultGuidValue);
			SyncManager manager = new SyncManager(null, null, null, null, null, null, serializer, guidService, null, null, null, null);
			Job job = GetJob(null);

			//ACT
			Guid returnValue = manager.GetBatchInstance(job);


			//ASSERT
			Assert.AreEqual(defaultGuidValue, returnValue);
		}

		[Test]
		public void GetBatchInstance_GuidInJobDetails_CorrectOutput()
		{
			//ARRANGE
			var serializer = NSubstitute.Substitute.For<kCura.Apps.Common.Utils.Serializers.JSONSerializer>();
			var guidService = NSubstitute.Substitute.For<IGuidService>();
			guidService.NewGuid().Returns(defaultGuidValue);
			SyncManager manager = new SyncManager(null, null, null, null, null, null, serializer, guidService, null, null, null, null);
			Job job = GetJob(serializer.Serialize(new TaskParameters() { BatchInstance = jobGuidValue }));

			//ACT
			Guid returnValue = manager.GetBatchInstance(job);


			//ASSERT
			Assert.AreEqual(jobGuidValue, returnValue);
		}

		[Test]
		public void GetBatchInstance_BadGuidInJobDetails_CorrectOutput()
		{
			//ARRANGE
			var serializer = NSubstitute.Substitute.For<kCura.Apps.Common.Utils.Serializers.JSONSerializer>();
			var guidService = NSubstitute.Substitute.For<IGuidService>();
			guidService.NewGuid().Returns(defaultGuidValue);
			SyncManager manager = new SyncManager(null, null, null, null, null, null, serializer, guidService, null, null, null, null);
			Job job = GetJob("BAD_GUID");

			//ACT
			
			Exception innerException = null;
			try
			{
				manager.GetBatchInstance(job);
			}
			catch (Exception ex)
			{
				innerException = ex;
			}


			//ASSERT
			Assert.AreEqual("Failed to get Batch Instance.", innerException.Message);
			Assert.IsNotNull(innerException.InnerException);
		}

		#endregion

		/*
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
		
		*/

		private Job GetJob(string jobDetails)
		{
			return JobHelper.GetJob(1, null, null, 1, 1, 111, 222, TaskType.SyncCustodianManagerWorker, new DateTime(), null, jobDetails,
				0, new DateTime(), 1, null, null);
		}
	}
}
