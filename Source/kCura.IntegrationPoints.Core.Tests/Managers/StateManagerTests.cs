using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class StateManagerTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			_instance = new StateManager();
		}

		private IStateManager _instance;

		[Test]
		public void GetNonRelativityProviderButtonState__JobsRunning_CanStop()
		{
			//Arrange
			bool hasJobsExecutingOrInQueue = true;
			bool hasStoppableJobs = true;
			bool hasErrors = true;
			bool hasViewPermissions = true;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Other, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsFalse(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.StopButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);

			//Assert Visible
			Assert.IsFalse(buttonStates.RetryErrorsButtonVisible);
			Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetNonRelativityProviderButtonState__NoJobsRunning_CantStop()
		{
			//Arrange
			bool hasJobsExecutingOrInQueue = false;
			bool hasStoppableJobs = false;
			bool hasErrors = true;
			bool hasViewPermissions = true;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Other, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);

			//Assert Visible
			Assert.IsFalse(buttonStates.RetryErrorsButtonVisible);
			Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetNonRelativityProviderButtonState__StoppingStage_CantStop()
		{
			//Arrange
			bool hasJobsExecutingOrInQueue = true;
			bool hasStoppableJobs = false;
			bool hasErrors = true;
			bool hasViewPermissions = true;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Other, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsFalse(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);

			//Assert Visible
			Assert.IsFalse(buttonStates.RetryErrorsButtonVisible);
			Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetRelativityProviderButtonState_ButtonsDisabled_JobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasJobsExecutingOrInQueue = true;
			bool hasViewPermissions = false;
			bool hasStoppableJobs = true;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Relativity, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsFalse(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsTrue(buttonStates.StopButtonEnabled);

			//Assert Visible
			Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
			Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetRelativityProviderButtonState_GoldFlow_HasErrors()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = true;
			bool hasStoppableJobs = false;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Relativity, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsTrue(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);

			//Assert Visible
			Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
			Assert.IsTrue(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetRelativityProviderButtonState_GoldFlow_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = false;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = true;
			bool hasStoppableJobs = false;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Relativity, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert Enable
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);

			//Assert Visible
			Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
			Assert.IsTrue(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetRelativityProviderButtonState_HasErrors_JobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = true;
			bool hasViewPermissions = false;
			bool hasStoppableJobs = true;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Relativity, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsFalse(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsTrue(buttonStates.StopButtonEnabled);

			//Assert Visible
			Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
			Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetRelativityProviderButtonState_HasErrors_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = true;
			bool hasStoppableJobs = false;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Relativity, hasJobsExecutingOrInQueue,
				hasErrors, hasViewPermissions, hasStoppableJobs, hasProfileAddPermission);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsTrue(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);

			//Assert Visible
			Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
			Assert.IsTrue(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetRelativityProviderButtonState_HasErrorsAndNoViewPermissions_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = false;
			bool hasStoppableJobs = false;
			bool hasProfileAddPermission = false;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Relativity, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);

			//Assert Visible
			Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
			Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
			Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetRelativityProviderButtonState_HasProfileAddPermission_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = false;
			bool hasStoppableJobs = false;
			bool hasProfileAddPermission = true;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Relativity, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);

			//Assert Visible
			Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
			Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
			Assert.IsTrue(buttonStates.SaveAsProfileButtonVisible);
		}

		[Test]
		public void GetOtherProviderButtonState_HasProfileAddPermission_NoJobsRunning()
		{
			//Arrange
			bool hasErrors = true;
			bool hasJobsExecutingOrInQueue = false;
			bool hasViewPermissions = false;
			bool hasStoppableJobs = false;
			bool hasProfileAddPermission = true;

			//Act
			ButtonStateDTO buttonStates = _instance.GetButtonState(Constants.SourceProvider.Other, hasJobsExecutingOrInQueue, hasErrors, hasViewPermissions, hasStoppableJobs,
				hasProfileAddPermission);

			//Assert
			Assert.IsTrue(buttonStates.RunButtonEnabled);
			Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
			Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
			Assert.IsFalse(buttonStates.StopButtonEnabled);

			//Assert Visible
			Assert.IsFalse(buttonStates.RetryErrorsButtonVisible);
			Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
			Assert.IsTrue(buttonStates.SaveAsProfileButtonVisible);
		}
	}
}