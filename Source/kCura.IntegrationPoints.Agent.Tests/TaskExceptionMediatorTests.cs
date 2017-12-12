using System;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture]
	public class TaskExceptionMediatorTests
	{
		private TaskExceptionMediator _subjectUnderTest;
		private ITaskExceptionService _taskExceptionServiceMock;
		private ScheduleQueueAgentBase _agentMock;

		[SetUp]
		public void SetUp()
		{
			_taskExceptionServiceMock = Substitute.For<ITaskExceptionService>();
			_agentMock = Substitute.For<Agent>();
		}

		[Test]
		public void ItShould_TriggerOnJobExecutionError()
		{
			//Arrange
			_subjectUnderTest = new TaskExceptionMediator(_taskExceptionServiceMock);
			_subjectUnderTest.RegisterEvent(_agentMock);

			//Act
			_agentMock.JobExecutionError += Raise.Event<ExceptionEventHandler>(null,null,new Exception());

			//Assert
			_taskExceptionServiceMock.Received(1).EndTaskWithError( Arg.Any<ITask>(), Arg.Any<Exception>());
		}

		[Test]
		public void ItShouldNot_TriggerOnJobExecutionError_AfterDispose()
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