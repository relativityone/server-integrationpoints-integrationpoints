using System;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Unit.IntegrationPoints
{
	[TestFixture]
	public class ConsoleEventHandlerTests
	{
		private const int _ARTIFACT_ID = 100300;
		private const int _APPLICATION_ID = 100101;

		private IManagerFactory _managerFactory;
		private IIntegrationPointManager _integrationPointManager;
		private IContextContainerFactory _contextContainerFactory;
		private IContextContainer _contextContainer;
		private IEHHelper _helper;
		private IFieldManager _fieldManager;
		private IJobHistoryManager _jobHistoryManager;
		private IArtifactGuidManager _artifactGuidManager;
		private IObjectTypeManager _objectTypeManager;

		private ConsoleEventHandler _instance;

		[SetUp]
		public void Setup()
		{
			_managerFactory = Substitute.For<IManagerFactory>();
			_integrationPointManager = Substitute.For<IIntegrationPointManager>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_contextContainer = Substitute.For<IContextContainer>();
			_helper = Substitute.For<IEHHelper>();
			_helper.GetActiveCaseID().Returns(_APPLICATION_ID);

			_fieldManager = Substitute.For<IFieldManager>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_artifactGuidManager = Substitute.For<IArtifactGuidManager>();
			_objectTypeManager = Substitute.For<IObjectTypeManager>();

			_managerFactory.CreateFieldManager(_contextContainer).Returns(_fieldManager);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_jobHistoryManager);
			_managerFactory.CreateArtifactGuidManager(_contextContainer).Returns(_artifactGuidManager);
			_managerFactory.CreateObjectTypeManager(_contextContainer).Returns(_objectTypeManager);

			var activeArtifact = new Artifact(_ARTIFACT_ID, null, 0, "", false, null);
			var application = new Application(_APPLICATION_ID, "", "");

			_instance = new EventHandlers.IntegrationPoints.ConsoleEventHandler(_contextContainerFactory, _managerFactory)
			{
				ActiveArtifact = activeArtifact,
				Application = application,
				Helper = _helper
			};
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void GetConsole_GoldFlow(bool isRelativitySourceProvider)
		{
			// ARRANGE
			string onClickEventForViewErrors = String.Empty;
			const int jobHistoryErrorStatusFieldArtifactId = 1076600;
			const int jobHistoryJobHistoryFieldArtifactId = 105055;
			const int jobHistoryErrorStatusNewChoiceArtifactId = 106002;
			const int jobHistoryErrorStatusArtifactViewFieldId = 500500;
			const int jobHistoryErrorDescriptorArtifactTypeId = 100042;
			const int jobHistoryArtifactViewFieldId = 500505;
			const int jobHistoryInstanceArtifactId = 506070;

			var integrationPointDto = new Contracts.Models.IntegrationPointDTO()
			{
				HasErrors = true,
				SourceProvider = 8392
			};

			_integrationPointManager.UserHasPermissions(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto)).Returns(true);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateIntegrationPointManager(_contextContainer).Returns(_integrationPointManager);


			_integrationPointManager.Read(_APPLICATION_ID, _ARTIFACT_ID).Returns(integrationPointDto);
			_integrationPointManager.IntegrationPointSourceProviderIsRelativity(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto))
				.Returns(isRelativitySourceProvider);

			if (isRelativitySourceProvider)
			{
				var errorErrorStatusFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.ErrorStatus);
				var jobHistoryFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.JobHistory);
				var guidsAndArtifactIds = new Dictionary<Guid, int>()
				{
					{JobHistoryErrorDTO.Choices.ErrorStatus.Guids.New, jobHistoryErrorStatusNewChoiceArtifactId},
					{errorErrorStatusFieldGuid, jobHistoryErrorStatusFieldArtifactId},
					{jobHistoryFieldGuid, jobHistoryJobHistoryFieldArtifactId}
				};

				_artifactGuidManager.GetArtifactIdsForGuids(Arg.Is(_APPLICATION_ID),
					Arg.Is<Guid[]>(x => x.Length == 3 
					&& x[0] == JobHistoryErrorDTO.Choices.ErrorStatus.Guids.New
					&& x[1] == errorErrorStatusFieldGuid
					&& x[2] == jobHistoryFieldGuid))
				.Returns(guidsAndArtifactIds);

				_fieldManager.RetrieveArtifactViewFieldId(_APPLICATION_ID, guidsAndArtifactIds[errorErrorStatusFieldGuid]).Returns(jobHistoryErrorStatusArtifactViewFieldId);
				_objectTypeManager.RetrieveObjectTypeDescriptorArtifactTypeId(_APPLICATION_ID, new Guid(JobHistoryErrorDTO.ArtifactTypeGuid)).Returns(jobHistoryErrorDescriptorArtifactTypeId);
				_fieldManager.RetrieveArtifactViewFieldId(_APPLICATION_ID, guidsAndArtifactIds[jobHistoryFieldGuid]).Returns(jobHistoryArtifactViewFieldId);
				_jobHistoryManager.GetLastJobHistoryArtifactId(_APPLICATION_ID, _ARTIFACT_ID).Returns(jobHistoryInstanceArtifactId);
				onClickEventForViewErrors = $"window.location='../../Case/IntegrationPoints/ErrorsRedirect.aspx?ErrorStatusArtifactViewFieldID={jobHistoryErrorStatusArtifactViewFieldId}"
						+ $"&ErrorStatusNewChoiceArtifactId={jobHistoryErrorStatusNewChoiceArtifactId}&JobHistoryErrorArtifactTypeId={jobHistoryErrorDescriptorArtifactTypeId}"
						+ $"&JobHistoryArtifactViewFieldID={jobHistoryArtifactViewFieldId}&JobHistoryInstanceArtifactId={jobHistoryInstanceArtifactId}'; return false;";
			}


			// ACT
			kCura.EventHandler.Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_contextContainerFactory.Received().CreateContextContainer(_helper);
			_integrationPointManager.Received(isRelativitySourceProvider ? 1 : 0).UserHasPermissions(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto));
			_managerFactory.Received().CreateIntegrationPointManager(_contextContainer);

			Assert.IsNotNull(console);
			Assert.AreEqual(isRelativitySourceProvider ? 3 : 1, console.ButtonList.Count);

			int buttonIndex = 0;
			ConsoleButton runNowButton = console.ButtonList[buttonIndex++];
			Assert.AreEqual("Run Now", runNowButton.DisplayText);
			Assert.AreEqual(true, runNowButton.Enabled);
			Assert.AreEqual(false, runNowButton.RaisesPostBack);
			Assert.AreEqual($"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})", runNowButton.OnClickEvent);

			if (isRelativitySourceProvider)
			{
				ConsoleButton retryErrorsButton = console.ButtonList[buttonIndex++];
				Assert.AreEqual("Retry Errors", retryErrorsButton.DisplayText);
				Assert.AreEqual(true, retryErrorsButton.Enabled);
				Assert.AreEqual(false, retryErrorsButton.RaisesPostBack);
				Assert.AreEqual($"IP.retryJob({_ARTIFACT_ID},{_APPLICATION_ID})", retryErrorsButton.OnClickEvent);

				ConsoleButton viewErrorsButtonLink = console.ButtonList[buttonIndex++];
				Assert.AreEqual("View Errors", viewErrorsButtonLink.DisplayText);
				Assert.AreEqual(true, viewErrorsButtonLink.Enabled);
				Assert.AreEqual(false, viewErrorsButtonLink.RaisesPostBack);
				Assert.AreEqual(onClickEventForViewErrors, viewErrorsButtonLink.OnClickEvent);
			}
		}
	}
}
