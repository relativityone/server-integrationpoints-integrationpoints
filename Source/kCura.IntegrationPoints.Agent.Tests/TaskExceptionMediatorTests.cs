using System;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture, Category("Unit")]
	public class TaskExceptionMediatorTests
	{
		private TaskExceptionMediator _subjectUnderTest;
		private ITaskExceptionService _taskExceptionServiceMock;
		private Agent _agentMock;

		[SetUp]
		public void SetUp()
		{
			_taskExceptionServiceMock = Substitute.For<ITaskExceptionService>();
			_agentMock = Substitute.For<Agent>();
		}

		[Test]
		public void ItShould_TriggerOnTaskExecutionError()
		{
			//Arrange
			_subjectUnderTest = new TaskExceptionMediator(_taskExceptionServiceMock);
			_subjectUnderTest.RegisterEvent(_agentMock);
			ITask task = Substitute.For<ITask>();

			//Act
			_agentMock.JobExecutionError += Raise.Event<ExceptionEventHandler>(null, task, new Exception());

			//Assert
			_taskExceptionServiceMock.Received(1).EndTaskWithError( Arg.Any<ITask>(), Arg.Any<Exception>());
		}

		[Test]
		public void ItShould_TriggerOnJobExecutionError()
		{
			//Arrange
			_subjectUnderTest = new TaskExceptionMediator(_taskExceptionServiceMock);
			_subjectUnderTest.RegisterEvent(_agentMock);
			Job job = JobExtensions.CreateJob();

			//Act
			_agentMock.JobExecutionError += Raise.Event<ExceptionEventHandler>(job, null, new Exception());

			//Assert
			_taskExceptionServiceMock.Received(1).EndJobWithError(Arg.Any<Job>(), Arg.Any<Exception>());
		}

		[Test]
		public void ItShouldNot_TriggerOnTaskExecutionError_AfterDispose()
		{
			//Arrange
			using (_subjectUnderTest = new TaskExceptionMediator(_taskExceptionServiceMock))
			{
				_subjectUnderTest.RegisterEvent(_agentMock);
			}// Dispose called

			//Act
			_agentMock.JobExecutionError += Raise.Event<ExceptionEventHandler>(null, null, new Exception());

			//Assert
			_taskExceptionServiceMock.Received(0).EndTaskWithError(Arg.Any<ITask>(), Arg.Any<Exception>());
		}
	}
}