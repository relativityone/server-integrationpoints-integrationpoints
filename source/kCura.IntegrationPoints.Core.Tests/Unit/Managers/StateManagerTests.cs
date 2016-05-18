using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class StateManagerTests
	{
		private IQueueRepository _queueRepository;
		private IRepositoryFactory _repositoryFactory;
		private IStateManager _instance;
		private int _workspaceId = 12345;
		private int _integrationPointId = 54321;

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_queueRepository = Substitute.For<IQueueRepository>();

			_repositoryFactory.GetQueueRepository().Returns(_queueRepository);

			_instance = new StateManager(_repositoryFactory);

			_repositoryFactory.Received().GetQueueRepository();
		}

		[Test]
		public void GetButtonState_GoldFlow_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasPermission = true;

			_queueRepository.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId).Returns(0);

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasPermission, hasErrors);

			//Assert
			Assert.IsTrue(buttonStates.RunNowButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			_queueRepository.Received(1).GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);
		}

		[Test]
		public void GetButtonState_ButtonsDisabled_JobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasPermission = true;

			_queueRepository.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId).Returns(1);

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasPermission, hasErrors);

			//Assert
			Assert.IsFalse(buttonStates.RunNowButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			_queueRepository.Received(1).GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);
		}

		[Test]
		public void GetButtonState_GoldFlow_HasErrors()
		{
			//Arrange
			bool hasErrors = true;
			bool hasPermission = true;

			_queueRepository.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId).Returns(0);

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasPermission, hasErrors);

			//Assert
			Assert.IsTrue(buttonStates.RunNowButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsTrue(buttonStates.ViewErrorsLinkEnabled);
			_queueRepository.Received(1).GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);
		}

		[Test]
		public void GetButtonState_HasErrors_JobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasPermission = true;

			_queueRepository.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId).Returns(5);

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasPermission, hasErrors);

			//Assert
			Assert.IsFalse(buttonStates.RunNowButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			_queueRepository.Received(1).GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);
		}

		[Test]
		public void GetButtonState_NoPermissions_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasPermission = false;

			_queueRepository.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId).Returns(0);

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasPermission, hasErrors);

			//Assert
			Assert.IsFalse(buttonStates.RunNowButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			_queueRepository.Received(1).GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);
		}
	}

	
}
