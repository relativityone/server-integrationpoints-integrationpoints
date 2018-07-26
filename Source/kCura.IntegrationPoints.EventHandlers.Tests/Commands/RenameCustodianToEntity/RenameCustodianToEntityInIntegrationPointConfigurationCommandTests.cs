using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity;
using Moq;
using NUnit.Framework;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands.RenameCustodianToEntity
{
	[TestFixture]
	public class RenameCustodianToEntityInIntegrationPointConfigurationCommandTests
	{
		private Mock<IIntegrationPointForSourceService> _integrationPointForSourceService;
		private Mock<IIntegrationPointService> _integrationPointService;

		[SetUp]
		public void SetUp()
		{
			_integrationPointForSourceService = new Mock<IIntegrationPointForSourceService>();
			_integrationPointService = new Mock<IIntegrationPointService>();

			var emptyIntegrationPointsList = new List<Data.IntegrationPoint>();
			_integrationPointForSourceService.Setup(service => service.GetAllForSourceProvider(It.IsAny<string>()))
				.Returns(emptyIntegrationPointsList);
		}

		[Test]
		public void ItShouldProcessIntegrationPointForAllNecessaryProviders()
		{
			// arrange
			RenameCustodianToEntityInIntegrationPointConfigurationCommand sut = CreateSut();

			// act
			sut.Execute();

			// assert
			string[] expectedSourceProviders =
			{
				Constants.IntegrationPoints.SourceProviders.LDAP,
				Constants.IntegrationPoints.SourceProviders.FTP,
				Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE
			};
			foreach (string provider in expectedSourceProviders)
			{
				_integrationPointForSourceService.Verify(
					service => service.GetAllForSourceProvider(provider),
					Times.Once,
					$"Integration Point for provider: {provider} should be processed.");
			}
		}

		private RenameCustodianToEntityInIntegrationPointConfigurationCommand CreateSut()
		{
			return new RenameCustodianToEntityInIntegrationPointConfigurationCommand(_integrationPointForSourceService.Object, _integrationPointService.Object);
		}
	}
}
