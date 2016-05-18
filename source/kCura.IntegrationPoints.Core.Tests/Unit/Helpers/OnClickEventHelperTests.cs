using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Helpers
{
	public class OnClickEventHelperTests
	{
		private IManagerFactory _managerFactory;
		private IContextContainerFactory _contextContainerFactory;
		private IContextContainer _contextContainer;
		private IFieldManager _fieldManager;
		private IJobHistoryManager _jobHistoryManager;
		private IArtifactGuidManager _artifactGuidManager;
		private IObjectTypeManager _objectTypeManager;
		private int _workspaceId = 12345;
		private int _integrationPointId = 54321;

		private OnClickEventHelper _instance;

		[SetUp]
		public void Setup()
		{
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_contextContainer = Substitute.For<IContextContainer>();
			_fieldManager = Substitute.For<IFieldManager>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_artifactGuidManager = Substitute.For<IArtifactGuidManager>();
			_objectTypeManager = Substitute.For<IObjectTypeManager>();

			_managerFactory.CreateFieldManager(_contextContainer).Returns(_fieldManager);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_jobHistoryManager);
			_managerFactory.CreateArtifactGuidManager(_contextContainer).Returns(_artifactGuidManager);
			_managerFactory.CreateObjectTypeManager(_contextContainer).Returns(_objectTypeManager);

			_instance = new OnClickEventHelper(_contextContainer, _managerFactory);
		}

		[Test]
		public void GetOnClickEventsForRelativityProviders_GoldFlow_AllButtonsEnabled()
		{
			//Arrange
			string expectedViewErrorsOnClickEvent = ViewErrorsLinkSetup();
			ButtonStateDTO buttonStates = new ButtonStateDTO()
			{
				RunNowButtonEnabled = true,
				RetryErrorsButtonEnabled = true,
				ViewErrorsLinkEnabled = true
			};

			//Act
			OnClickEventDTO onClickEvents = _instance.GetOnClickEventsForRelativityProvider(_workspaceId, _integrationPointId,
				buttonStates);
			
			//Assert
			Assert.IsTrue(onClickEvents.RunNowOnClickEvent == $"IP.importNow({_integrationPointId},{_workspaceId})");
			Assert.IsTrue(onClickEvents.RetryErrorsOnClickEvent == $"IP.retryJob({_integrationPointId},{_workspaceId})");
			Assert.IsTrue(onClickEvents.ViewErrorsOnClickEvent == expectedViewErrorsOnClickEvent);
		}

		[Test]
		public void GetOnClickEventsForRelativityProviders_GoldFlow_AllButtonsDisabled()
		{
			//Arrange
			ButtonStateDTO buttonStates = new ButtonStateDTO()
			{
				RunNowButtonEnabled = false,
				RetryErrorsButtonEnabled = false,
				ViewErrorsLinkEnabled = false
			};

			//Act
			OnClickEventDTO onClickEvents = _instance.GetOnClickEventsForRelativityProvider(_workspaceId, _integrationPointId,
				buttonStates);

			//Assert
			Assert.IsTrue(onClickEvents.RunNowOnClickEvent == String.Empty);
			Assert.IsTrue(onClickEvents.RetryErrorsOnClickEvent == String.Empty);
			Assert.IsTrue(onClickEvents.ViewErrorsOnClickEvent == String.Empty);
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

			var errorErrorStatusFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.ErrorStatus);
			var jobHistoryFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.JobHistory);
			var guidsAndArtifactIds = new Dictionary<Guid, int>()
				{
					{JobHistoryErrorDTO.Choices.ErrorStatus.Guids.New, jobHistoryErrorStatusNewChoiceArtifactId},
					{errorErrorStatusFieldGuid, jobHistoryErrorStatusFieldArtifactId},
					{jobHistoryFieldGuid, jobHistoryJobHistoryFieldArtifactId}
				};

			_artifactGuidManager.GetArtifactIdsForGuids(Arg.Is(_workspaceId),
				Arg.Is<Guid[]>(x => x.Length == 3
				&& x[0] == JobHistoryErrorDTO.Choices.ErrorStatus.Guids.New
				&& x[1] == errorErrorStatusFieldGuid
				&& x[2] == jobHistoryFieldGuid))
			.Returns(guidsAndArtifactIds);

			_fieldManager.RetrieveArtifactViewFieldId(_workspaceId, guidsAndArtifactIds[errorErrorStatusFieldGuid]).Returns(jobHistoryErrorStatusArtifactViewFieldId);
			_objectTypeManager.RetrieveObjectTypeDescriptorArtifactTypeId(_workspaceId, new Guid(JobHistoryErrorDTO.ArtifactTypeGuid)).Returns(jobHistoryErrorDescriptorArtifactTypeId);
			_fieldManager.RetrieveArtifactViewFieldId(_workspaceId, guidsAndArtifactIds[jobHistoryFieldGuid]).Returns(jobHistoryArtifactViewFieldId);
			_jobHistoryManager.GetLastJobHistoryArtifactId(_workspaceId, _integrationPointId).Returns(jobHistoryInstanceArtifactId);
			string onClickEventForViewErrors = $"window.location='../../Case/IntegrationPoints/ErrorsRedirect.aspx?ErrorStatusArtifactViewFieldID={jobHistoryErrorStatusArtifactViewFieldId}"
					+ $"&ErrorStatusNewChoiceArtifactId={jobHistoryErrorStatusNewChoiceArtifactId}&JobHistoryErrorArtifactTypeId={jobHistoryErrorDescriptorArtifactTypeId}"
					+ $"&JobHistoryArtifactViewFieldID={jobHistoryArtifactViewFieldId}&JobHistoryInstanceArtifactId={jobHistoryInstanceArtifactId}'; return false;";

			return onClickEventForViewErrors;
		}
	}
}
