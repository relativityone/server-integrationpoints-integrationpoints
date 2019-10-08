using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
		private CommonScripts _sut;
		private Mock<IScriptsHelper> _scriptsHelper;
		private Mock<IIntegrationPointBaseFieldGuidsConstants> _integrationPointBaseFieldGuidsConstants;

		private const string _INTEGRATION_POINTS_CONTROLLER_NAME = "IntegrationPointsAPI";
		private const string _INTEGRATION_POINTS_PROFILE_CONTROLLER_NAME = "IntegrationPointProfilesAPI";

		private readonly List<string> _allScripts = new List<string>
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

		private readonly List<string> _signalRScripts = new List<string>
		{
			"/Scripts/jquery.signalR-2.3.0.js",
			"/signalr/hubs",
			"/Scripts/hubs/integrationPointHub.js",
		};

		[SetUp]
		public void SetUp()
		{
			_scriptsHelper = new Mock<IScriptsHelper>();
			_integrationPointBaseFieldGuidsConstants = new Mock<IIntegrationPointBaseFieldGuidsConstants>();

			_sut = new CommonScripts(_scriptsHelper.Object, _integrationPointBaseFieldGuidsConstants.Object);
		}

		[Test]
		public void ItShouldReturnAllScriptsFroIntegrationPointPage()
		{
			//Arrange
			_scriptsHelper.Setup(x => x.GetAPIControllerName()).Returns(_INTEGRATION_POINTS_CONTROLLER_NAME);

			//Act
			IList<string> linkedScripts = _sut.LinkedScripts();

			//Assert
			linkedScripts.Should().Equal(_allScripts);
		}

		[Test]
		public void ItShouldReturnAllScriptsFroIntegrationPointProfilePage()
		{
			//Arrange
			_scriptsHelper.Setup(x => x.GetAPIControllerName()).Returns(_INTEGRATION_POINTS_PROFILE_CONTROLLER_NAME);

			//Act
			IList<string> linkedScripts = _sut.LinkedScripts();

			//Assert
			linkedScripts.Should().Equal(_allScripts.Except(_signalRScripts));
		}
	}
}