using System;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture]
	public class TaskExceptionServiceTests
	{
		private TaskExceptionService _subjectUnderTest;
		private IJobHistoryErrorService _jobHistoryErrorServiceMock;
		private IJobHistoryService _jobHistoryServiceMock;
		private IJobService _jobServiceMock;
		private JobHistory _jobHistoryDto;

		[SetUp]
		public void SetUp()
		{
			_jobHistoryErrorServiceMock = Substitute.For<IJobHistoryErrorService>();
			_jobHistoryServiceMock = Substitute.For<IJobHistoryService>();
			_jobServiceMock = Substitute.For<IJobService>();
			_jobHistoryDto = new JobHistory();
			_subjectUnderTest = new TaskExceptionService(_jobHistoryErrorServiceMock,_jobHistoryServiceMock,_jobServiceMock);
		}

		[Test]
		public void ItShould_EndTaskWithError_ForITaskWithHistory()
		{
			//Arange
			var task = Substitute.For<ITaskWithJobHistory>();
			task.JobHistory.Returns(_jobHistoryDto);
			var exception = new IntegrationPointsException("Job failed miserably.");

			//Act
			_subjectUnderTest.EndTaskWithError(task, exception);

			//Assert
			Assert.AreEqual(JobStatusChoices.JobHistoryErrorJobFailed.Name, _jobHistoryDto.JobStatus.Name);
			_jobHistoryErrorServiceMock.Received(1).AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, exception.Message, exception.StackTrace );
			_jobHistoryServiceMock.Received(1).UpdateRdo(_jobHistoryDto);
			_jobServiceMock.Received(1).CleanupJobQueueTable(); 
		}
	}
}