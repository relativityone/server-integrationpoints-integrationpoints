using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
	[TestFixture]
	public class OnClickEventConstructorTests : TestBase
	{
		private IManagerFactory _managerFactory;
		private IContextContainer _contextContainer;
		private IFieldManager _fieldManager;
		private IJobHistoryManager _jobHistoryManager;
		private IArtifactGuidManager _artifactGuidManager;
		private IObjectTypeManager _objectTypeManager;
		private int _workspaceId = 12345;
		private int _integrationPointId = 54321;
		private string _integrationPointName = "ip_258";

		private OnClickEventConstructor _instance;

		[SetUp]
		public override void SetUp()
		{
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainer = Substitute.For<IContextContainer>();
			_fieldManager = Substitute.For<IFieldManager>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_artifactGuidManager = Substitute.For<IArtifactGuidManager>();
			_objectTypeManager = Substitute.For<IObjectTypeManager>();

			_managerFactory.CreateFieldManager(_contextContainer).Returns(_fieldManager);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_jobHistoryManager);
			_managerFactory.CreateArtifactGuidManager(_contextContainer).Returns(_artifactGuidManager);
			_managerFactory.CreateObjectTypeManager(_contextContainer).Returns(_objectTypeManager);

			_instance = new OnClickEventConstructor(_contextContainer, _managerFactory);
		}

		[Test]
		public void GetOnClickEventsForRelativityProvider_GoldFlow_AllButtonsEnabled()
		{
			//Arrange
			string expectedViewErrorsOnClickEvent = ViewErrorsLinkSetup();
			var buttonStates = new ButtonStateDTO()
			{
				RunButtonEnabled = true,
				RetryErrorsButtonEnabled = true,
				ViewErrorsLinkEnabled = true,
				StopButtonEnabled = true
			};

			//Act
			OnClickEventDTO onClickEvents = _instance.GetOnClickEvents(_workspaceId, _integrationPointId, _integrationPointName, buttonStates);

			//Assert
			Assert.IsTrue(onClickEvents.RunOnClickEvent == $"IP.importNow({_integrationPointId},{_workspaceId})");
			Assert.IsTrue(onClickEvents.RetryErrorsOnClickEvent == $"IP.retryJob({_integrationPointId},{_workspaceId})");
			Assert.IsTrue(onClickEvents.ViewErrorsOnClickEvent == expectedViewErrorsOnClickEvent);
			Assert.IsTrue(onClickEvents.StopOnClickEvent == $"IP.stopJob({_integrationPointId},{_workspaceId})");
			Assert.IsTrue(onClickEvents.SaveAsProfileOnClickEvent == $"IP.saveAsProfile({_integrationPointId},{_workspaceId},'{_integrationPointName}')");
		}

		[Test]
		public void GetOnClickEventsForRelativityProvider_GoldFlow_AllButtonsDisabled()
		{
			//Arrange
			var buttonStates = new ButtonStateDTO()
			{
				RunButtonEnabled = false,
				RetryErrorsButtonEnabled = false,
				ViewErrorsLinkEnabled = false,
				StopButtonEnabled = false
			};

			//Act
			OnClickEventDTO onClickEvents = _instance.GetOnClickEvents(_workspaceId, _integrationPointId, _integrationPointName, buttonStates);

			//Assert
			Assert.IsTrue(onClickEvents.RunOnClickEvent == String.Empty);
			Assert.IsTrue(onClickEvents.RetryErrorsOnClickEvent == String.Empty);
			Assert.IsTrue(onClickEvents.ViewErrorsOnClickEvent == String.Empty);
			Assert.IsTrue(onClickEvents.StopOnClickEvent == String.Empty);
			Assert.IsTrue(onClickEvents.SaveAsProfileOnClickEvent == $"IP.saveAsProfile({_integrationPointId},{_workspaceId},'{_integrationPointName}')");
		}

		[Test]
		public void GetOnClickEventsForNonRelativityProvider_GoldFlow()
		{
			//Arrange
			var buttonStates = new ButtonStateDTO()
			{
				RunButtonEnabled = true,
				StopButtonEnabled = false
			};

			//Act
			OnClickEventDTO onClickEvents = _instance.GetOnClickEvents(_workspaceId, _integrationPointId, _integrationPointName, buttonStates);

			//Assert
			Assert.AreEqual($"IP.importNow({_integrationPointId},{_workspaceId})", onClickEvents.RunOnClickEvent);
			Assert.AreEqual(String.Empty, onClickEvents.StopOnClickEvent);
		}

		[Test]
		public void GetOnClickEventsForNonRelativityProvider_StopButtonEnabled()
		{
			//Arrange
			var buttonStates = new ButtonStateDTO()
			{
				RunButtonEnabled = false,
				StopButtonEnabled = true
			};

			//Act
			OnClickEventDTO onClickEvents = _instance.GetOnClickEvents(_workspaceId, _integrationPointId, _integrationPointName, buttonStates);

			//Assert
			Assert.AreEqual(String.Empty, onClickEvents.RunOnClickEvent);
			Assert.AreEqual($"IP.stopJob({_integrationPointId},{_workspaceId})", onClickEvents.StopOnClickEvent);
		}

		[Test]
		public void GetOnClickEventsForNonRelativityProvider_NeitherButtonEnabled()
		{
			//Arrange
			var buttonStates = new ButtonStateDTO()
			{
				RunButtonEnabled = false,
				StopButtonEnabled = false
			};

			//Act
			OnClickEventDTO onClickEvents = _instance.GetOnClickEvents(_workspaceId, _integrationPointId, _integrationPointName, buttonStates);

			//Assert
			Assert.AreEqual(String.Empty, onClickEvents.RunOnClickEvent);
			Assert.AreEqual(String.Empty, onClickEvents.StopOnClickEvent);
		}


		private string ViewErrorsLinkSetup()
		{
			const int jobHistoryErrorStatusFieldArtifactId = 1076600;
			const int jobHistoryJobHistoryFieldArtifactId = 105055;
			const int jobHistoryErrorStatusNewChoiceArtifactId = 106002;
			const int jobHistoryErrorStatusArtifactViewFieldId = 500500;
			const int jobHistoryErrorDescriptorArtifactTypeId = 100042;
			const int jobHistoryArtifactViewFieldId = 500505;
			const int jobHistoryInstanceArtifactId = 506070;

			var errorErrorStatusFieldGuid = new Guid(JobHistoryErrorFieldGuids.ErrorStatus);
			var jobHistoryFieldGuid = new Guid(JobHistoryErrorFieldGuids.JobHistory);
			var guidsAndArtifactIds = new Dictionary<Guid, int>()
				{
					{ErrorStatusChoices.JobHistoryErrorNew.Guids.Single(), jobHistoryErrorStatusNewChoiceArtifactId},
					{errorErrorStatusFieldGuid, jobHistoryErrorStatusFieldArtifactId},
					{jobHistoryFieldGuid, jobHistoryJobHistoryFieldArtifactId}
				};

			_artifactGuidManager.GetArtifactIdsForGuids(Arg.Is(_workspaceId),
				Arg.Is<Guid[]>(x => x.Length == 3
				&& x[0] == ErrorStatusChoices.JobHistoryErrorNew.Guids.Single()
				&& x[1] == errorErrorStatusFieldGuid
				&& x[2] == jobHistoryFieldGuid))
			.Returns(guidsAndArtifactIds);

			_fieldManager.RetrieveArtifactViewFieldId(_workspaceId, guidsAndArtifactIds[errorErrorStatusFieldGuid]).Returns(jobHistoryErrorStatusArtifactViewFieldId);
			_objectTypeManager.RetrieveObjectTypeDescriptorArtifactTypeId(_workspaceId, new Guid(ObjectTypeGuids.JobHistoryError)).Returns(jobHistoryErrorDescriptorArtifactTypeId);
			_fieldManager.RetrieveArtifactViewFieldId(_workspaceId, guidsAndArtifactIds[jobHistoryFieldGuid]).Returns(jobHistoryArtifactViewFieldId);
			_jobHistoryManager.GetLastJobHistoryArtifactId(_workspaceId, _integrationPointId).Returns(jobHistoryInstanceArtifactId);
			string onClickEventForViewErrors = $"window.location='../../Case/IntegrationPoints/ErrorsRedirect.aspx?ErrorStatusArtifactViewFieldID={jobHistoryErrorStatusArtifactViewFieldId}"
					+ $"&ErrorStatusNewChoiceArtifactId={jobHistoryErrorStatusNewChoiceArtifactId}&JobHistoryErrorArtifactTypeId={jobHistoryErrorDescriptorArtifactTypeId}"
					+ $"&JobHistoryArtifactViewFieldID={jobHistoryArtifactViewFieldId}&JobHistoryInstanceArtifactId={jobHistoryInstanceArtifactId}'; return false;";

			return onClickEventForViewErrors;
		}
	}
}
