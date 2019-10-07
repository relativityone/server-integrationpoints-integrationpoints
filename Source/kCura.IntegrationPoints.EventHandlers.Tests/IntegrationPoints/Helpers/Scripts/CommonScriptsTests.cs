using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers.Scripts
{
	[TestFixture]

	public class CommonScriptsTests
	{
		private CommonScripts _instance;
		private Mock<IScriptsHelper> _scriptsHelper;
		private Mock<IIntegrationPointBaseFieldGuidsConstants> _integrationPointBaseFieldGuidsConstants;

		[SetUp]
		public void SetUp()
		{
			_scriptsHelper = new Mock<IScriptsHelper>();
			_integrationPointBaseFieldGuidsConstants = new Mock<IIntegrationPointBaseFieldGuidsConstants>();

			_instance = new CommonScripts(_scriptsHelper.Object, _integrationPointBaseFieldGuidsConstants.Object);
		}

		[Test]
		public void ItShouldReturnListOfLinkedScriptsWithSignalrAndHub()
		{
			//Arrange
			_scriptsHelper.Setup(x => x.GetAPIControllerName()).Returns("IntegrationPointsAPI");

			//Act
			IList<string> listOfLinkedScripts = _instance.LinkedScripts();

			//Assert
			listOfLinkedScripts.Count.Should().Be(19);
			listOfLinkedScripts.Should().Equal(listWithAllScripts());
		}

		[Test]
		public void ItShouldReturnListOfLinkedScriptsWithoutSignalrAndHub()
		{
			//Arrange
			_scriptsHelper.Setup(x => x.GetAPIControllerName()).Returns("IntegrationPointProfilesAPI");

			//Act
			IList<string> listOfLinkedScripts = _instance.LinkedScripts();

			//Assert
			listOfLinkedScripts.Count.Should().Be(16);
			listOfLinkedScripts.Should().Equal(listWithoutHubAndSignalrSCripts());
		}

		public IList<string> listWithAllScripts()
		{
			return new List<string>
			{
				"/Scripts/knockout-3.4.0.js",
				"/Scripts/knockout.validation.js",
				"/Scripts/route.js",
				"/Scripts/date.js",
				"/Scripts/q.js",
				"/Scripts/core/messaging.js",
				"/Scripts/loading-modal.js",
				"/Scripts/dragon/dragon-dialogs.js",
				"/Scripts/core/data.js",
				"/Scripts/core/utils.js",
				"/Scripts/integration-point/time-utils.js",
				"/Scripts/integration-point/picker.js",
				"/Scripts/Export/export-validation.js",
				"/Scripts/integration-point/save-as-profile-modal-vm.js",
				"/Scripts/jquery.signalR-2.3.0.js",
				"/signalr/hubs",
				"/Scripts/hubs/integrationPointHub.js",
				"/Scripts/EventHandlers/integration-points-view-destination.js",
				"/Scripts/EventHandlers/integration-points-summary-page-view.js"
			};
		}

		public IList<string> listWithoutHubAndSignalrSCripts()
		{
			return new List<string>
			{
				"/Scripts/knockout-3.4.0.js",
				"/Scripts/knockout.validation.js",
				"/Scripts/route.js",
				"/Scripts/date.js",
				"/Scripts/q.js",
				"/Scripts/core/messaging.js",
				"/Scripts/loading-modal.js",
				"/Scripts/dragon/dragon-dialogs.js",
				"/Scripts/core/data.js",
				"/Scripts/core/utils.js",
				"/Scripts/integration-point/time-utils.js",
				"/Scripts/integration-point/picker.js",
				"/Scripts/Export/export-validation.js",
				"/Scripts/integration-point/save-as-profile-modal-vm.js",
				"/Scripts/EventHandlers/integration-points-view-destination.js",
				"/Scripts/EventHandlers/integration-points-summary-page-view.js"
			};
		}
	}
}