using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class StateManagerTests
	{
		private IStateManager _instance;

		[SetUp]
		public void Setup()
		{
			_instance = new StateManager();
		}

		[Test]
		public void GetRelativityProviderButtonState_GoldFlow_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = true;
			bool hasStoppableJobs = false;

			//Act
			RelativityButtonStateDTO buttonStates = _instance.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);
		}

		[Test]
		public void GetRelativityProviderButtonState_ButtonsDisabled_JobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasJobsExecutingOrInQueue = true;
			bool hasViewPermissions = false;
			bool hasStoppableJobs = true;

			//Act
			RelativityButtonStateDTO buttonStates = _instance.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs);

			//Assert
			Assert.IsFalse(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsTrue(buttonStates.StopButtonEnabled);
		}

		[Test]
		public void GetRelativityProviderButtonState_GoldFlow_HasErrors()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = true;
			bool hasStoppableJobs = false;

			//Act
			RelativityButtonStateDTO buttonStates = _instance.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsTrue(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);
		}

		[Test]
		public void GetRelativityProviderButtonState_HasErrors_JobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = true;
			bool hasViewPermissions = false;
			bool hasStoppableJobs = true;

			//Act
			RelativityButtonStateDTO buttonStates = _instance.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs);

			//Assert
			Assert.IsFalse(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsTrue(buttonStates.StopButtonEnabled);
		}

		[Test]
		public void GetRelativityProviderButtonState_HasErrors_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = true;
			bool hasStoppableJobs = false;

			//Act
			RelativityButtonStateDTO buttonStates = _instance.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue,
				hasErrors, hasViewPermissions, hasStoppableJobs);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsTrue(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);
		}

		[Test]
		public void GetRelativityProviderButtonState_HasErrorsAndNoViewPermissions_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = false;
			bool hasStoppableJobs = false;

			//Act
			RelativityButtonStateDTO buttonStates = _instance.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);
		}

		[Test]
		public void GetNonRelativityProviderButtonState__NoJobsRunning_CantStop()
		{
			//Arrange
			bool hasJobsExecutingOrInQueue = false;
			bool hasStoppableJobs = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(hasJobsExecutingOrInQueue, hasStoppableJobs);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);
		}

		[Test]
		public void GetNonRelativityProviderButtonState__JobsRunning_CanStop()
		{
			//Arrange
			bool hasJobsExecutingOrInQueue = true;
			bool hasStoppableJobs = true;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(hasJobsExecutingOrInQueue, hasStoppableJobs);

			//Assert
			Assert.IsFalse(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.StopButtonEnabled);
		}

		[Test]
		public void GetNonRelativityProviderButtonState__StoppingStage_CantStop()
		{
			//Arrange
			bool hasJobsExecutingOrInQueue = true;
			bool hasStoppableJobs = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(hasJobsExecutingOrInQueue, hasStoppableJobs);

			//Assert
			Assert.IsFalse(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);
		}
	}	
}
