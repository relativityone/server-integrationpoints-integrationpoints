using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity;
using kCura.Relativity.Client.DTOs;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands.RenameCustodianToEntity
{
	[TestFixture]
	public class RenameCustodianToEntityForSourceProviderCommandTests
	{
		private Mock<IIntegrationPointForSourceService> _integrationPointForSourceService;
		private Mock<IIntegrationPointService> _integrationPointService;
		private string _sourceProviderGuid;

		[SetUp]
		public void SetUp()
		{
			_integrationPointForSourceService = new Mock<IIntegrationPointForSourceService>();
			_integrationPointService = new Mock<IIntegrationPointService>();
			_sourceProviderGuid = Guid.NewGuid().ToString();
		}

		[Test]
		public void ItShouldWork_ForEmptyIntegrationPointCollection()
		{
			// arrange
			var sourceIntegrationPoints = new List<Data.IntegrationPoint>();
			_integrationPointForSourceService.Setup(x => x.GetAllForSourceProvider(_sourceProviderGuid)).Returns(sourceIntegrationPoints);
			RenameCustodianToEntityForSourceProviderCommand sut = CreateSut(_sourceProviderGuid);

			// act
			sut.Execute();
		}

		[Test]
		public void ItShouldUpdateIntegrationPoint_WhenRenamedFieldIsPresentInConfiguration()
		{
			// arrange
			int artifactId = 53423;
			var sourceIntegrationPoints = new List<Data.IntegrationPoint> { CreateIntegrationPointWithRenamedProperty(artifactId) };
			_integrationPointForSourceService.Setup(x => x.GetAllForSourceProvider(_sourceProviderGuid)).Returns(sourceIntegrationPoints);
			RenameCustodianToEntityForSourceProviderCommand sut = CreateSut(_sourceProviderGuid);

			// act
			sut.Execute();

			// assert
			AssertIntegrationPointWasUpdated(artifactId);
		}

		[Test]
		public void ItShouldNotUpdateIntegrationPoint_WhenRenamedFieldIsNotPresentInConfiguration()
		{
			// arrange
			int artifactId = 3232;
			var sourceIntegrationPoints = new List<Data.IntegrationPoint> { CreateIntegrationPointWithoutRenamedProperty(artifactId) };
			_integrationPointForSourceService.Setup(x => x.GetAllForSourceProvider(_sourceProviderGuid)).Returns(sourceIntegrationPoints);
			RenameCustodianToEntityForSourceProviderCommand sut = CreateSut(_sourceProviderGuid);

			// act
			sut.Execute();

			// assert
			AssertIntegrationPointWasNotUpdated(artifactId);
		}

		[Test]
		public void ItShouldWork_ForHeterogeneousIntegretionPointCollection()
		{
			// arrange
			int artifactIdForIntegrationPointWithoutCustodian = 4942;
			int[] artifactsIdForIntegrationPointWithCustodian = { 5432, 98922 };

			var sourceIntegrationPoints = new List<Data.IntegrationPoint>
			{
				CreateIntegrationPointWithRenamedProperty(artifactsIdForIntegrationPointWithCustodian[0]),
				CreateIntegrationPointWithoutRenamedProperty(artifactIdForIntegrationPointWithoutCustodian),
				CreateIntegrationPointWithRenamedProperty(artifactsIdForIntegrationPointWithCustodian[1])
			};
			_integrationPointForSourceService.Setup(x => x.GetAllForSourceProvider(_sourceProviderGuid)).Returns(sourceIntegrationPoints);
			RenameCustodianToEntityForSourceProviderCommand sut = CreateSut(_sourceProviderGuid);

			// act
			sut.Execute();

			// assert
			AssertIntegrationPointWasUpdated(artifactsIdForIntegrationPointWithCustodian[0]);
			AssertIntegrationPointWasUpdated(artifactsIdForIntegrationPointWithCustodian[1]);
			AssertIntegrationPointWasNotUpdated(artifactIdForIntegrationPointWithoutCustodian);
		}

		private RenameCustodianToEntityForSourceProviderCommand CreateSut(string providerGuid)
		{
			return new RenameCustodianToEntityForSourceProviderCommand(providerGuid, _integrationPointForSourceService.Object, _integrationPointService.Object);
		}

		private Data.IntegrationPoint CreateIntegrationPointWithoutRenamedProperty(int artifactId)
		{
			Data.IntegrationPoint ip = CreateIntegrationPoint(artifactId);
			ip.DestinationConfiguration = "{\"PropertyA\":\"value\"}";
			return ip;
		}

		private Data.IntegrationPoint CreateIntegrationPointWithRenamedProperty(int artifactId)
		{
			Data.IntegrationPoint ip = CreateIntegrationPoint(artifactId);
			ip.DestinationConfiguration = "{\"PropertyA\":\"value\",\"CustodianManagerFieldContainsLink\":\"v2\"}";
			return ip;
		}

		private Data.IntegrationPoint CreateIntegrationPoint(int artifactId)
		{
			return new Data.IntegrationPoint
			{
				ArtifactId = artifactId,
				DestinationConfiguration = string.Empty,
				Name = string.Empty,
				OverwriteFields = new Choice(1) { Name = string.Empty },
				SourceConfiguration = string.Empty,
				SourceProvider = 0,
				Type = 0,
				FieldMappings = string.Empty,
				EnableScheduler = false,
				DestinationProvider = 0,
				LogErrors = false,
				HasErrors = false,
				EmailNotificationRecipients = string.Empty,
				LastRuntimeUTC = DateTime.UtcNow,
				NextScheduledRuntimeUTC = DateTime.UtcNow,
				SecuredConfiguration = string.Empty,
				PromoteEligible = false,
				ScheduleRule = string.Empty
			};
		}

		private void AssertIntegrationPointWasUpdated(int artifactId)
		{
			_integrationPointService.Verify(
				service => service.SaveIntegration(It.Is<IntegrationPointModel>(ip => ip.ArtifactID == artifactId)),
				Times.Once,
				"Integration Point configuration should be updated.");
		}

		private void AssertIntegrationPointWasNotUpdated(int artifactId)
		{
			_integrationPointService.Verify(
				service => service.SaveIntegration(It.Is<IntegrationPointModel>(ip => ip.ArtifactID == artifactId)),
				Times.Never,
				"Integration Point configuration should not be updated.");
		}
	}
}
