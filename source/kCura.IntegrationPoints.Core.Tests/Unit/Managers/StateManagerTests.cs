using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class StateManagerTests
	{
		private IStateManager _instance;
		private int _workspaceId = 12345;
		private int _integrationPointId = 54321;

		[SetUp]
		public void Setup()
		{
			_instance = new StateManager();
		}

		[Test]
		public void GetButtonState_GoldFlow_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = true;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions);

			//Assert
			Assert.IsTrue(buttonStates.RunNowButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
		}

		[Test]
		public void GetButtonState_ButtonsDisabled_JobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasJobsExecutingOrInQueue = true;
			bool hasViewPermissions = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions);

			//Assert
			Assert.IsFalse(buttonStates.RunNowButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
		}

		[Test]
		public void GetButtonState_GoldFlow_HasErrors()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = true;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions);

			//Assert
			Assert.IsTrue(buttonStates.RunNowButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsTrue(buttonStates.ViewErrorsLinkEnabled);
		}

		[Test]
		public void GetButtonState_HasErrors_JobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = true;
			bool hasViewPermissions = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions);

			//Assert
			Assert.IsFalse(buttonStates.RunNowButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
		}

		[Test]
		public void GetButtonState_HasErrorsAndNoViewPermissions_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(_workspaceId, _integrationPointId, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions);

			//Assert
			Assert.IsTrue(buttonStates.RunNowButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
		}
	}	
}
