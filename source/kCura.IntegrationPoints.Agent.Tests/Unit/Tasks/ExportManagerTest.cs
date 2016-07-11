using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	public class ExportManagerTest
	{
		#region Private Fields

		private ExportManager _instanceToTest;

		private IJobManager _jobManagerMock;
		private ICaseServiceContext _caseServiceContextMock;

		private Job _job = JobHelper.GetJob(1, 2, 3, 4, 5, 6, 7, TaskType.ExportWorker,
				DateTime.MinValue, DateTime.MinValue, null, 1, DateTime.MinValue, 2, "", null); 

		#endregion //Private Fields

		[SetUp]
		public void Init()
		{
			_jobManagerMock = Substitute.For<IJobManager>();
			_caseServiceContextMock = Substitute.For<ICaseServiceContext>();

			_instanceToTest = new ExportManager(Substitute.For<ICaseServiceContext>(),
				Substitute.For<IDataProviderFactory>(),
				_jobManagerMock,
				Substitute.For<IJobService>(),
				Substitute.For<IHelper>(),
				Substitute.For<IIntegrationPointService>(),
				Substitute.For<ISerializer>(),
				Substitute.For<IGuidService>(),
				Substitute.For<IJobHistoryService>(),
				Substitute.For<JobHistoryErrorService>(_caseServiceContextMock),
				Substitute.For<IScheduleRuleFactory>(),
				new List<IBatchStatus>());
		}

		[Test]
		public void ItShouldReturnExportWorker()
		{
			_instanceToTest.CreateBatchJob(_job, new List<string>());

			_jobManagerMock.Received().CreateJobWithTracker(_job, Arg.Any<TaskParameters>(), TaskType.ExportWorker, Arg.Any<string>());
		}

		[Test]
		public void ItShouldReturnOneBatchId()
		{
			var batchId = _instanceToTest.GetUnbatchedIDs(_job).ToList();

			Assert.That(batchId.Count, Is.EqualTo(1));
			Assert.That(batchId[0], Is.EqualTo(_job.JobId.ToString()));
		}
	}
}

















