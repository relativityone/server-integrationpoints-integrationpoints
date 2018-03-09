using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoint.Tests.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class UpdateRelativityConfigurationCommandTests : TestBase
	{
		private UpdateRelativityConfigurationCommand _command;
		private IIntegrationPointForSourceService _integrationPointForSourceService;
		private IIntegrationPointService _integrationPointService;
		private IRemoveSecuredConfigurationFromIntegrationPointService _removeSecuredConfigurationService;
		private Data.IntegrationPoint _integrationPointWithConfiguration;
		private Data.IntegrationPoint _integrationPointWithoutConfiguration;

		public override void SetUp()
		{
			_integrationPointForSourceService = Substitute.For<IIntegrationPointForSourceService>();
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_removeSecuredConfigurationService = Substitute.For<IRemoveSecuredConfigurationFromIntegrationPointService>();

			_command = new UpdateRelativityConfigurationCommand(_integrationPointForSourceService, _integrationPointService, _removeSecuredConfigurationService);
			_integrationPointWithConfiguration =
				new Data.IntegrationPoint
			{
				ArtifactId = 1,
				Name = "Name",
				OverwriteFields = new Choice(1) { Name = "Name" },
				SourceConfiguration = "",
				SourceProvider = 1,
				Type = 1,
				DestinationConfiguration = "",
				FieldMappings = "",
				EnableScheduler = false,
				DestinationProvider = 1,
				LogErrors = true,
				HasErrors = false,
				EmailNotificationRecipients = "",
				LastRuntimeUTC = DateTime.UtcNow,
				NextScheduledRuntimeUTC = DateTime.UtcNow,
				SecuredConfiguration = "securedConfiguration",
				PromoteEligible = true,
				ScheduleRule = ""
			};

			_integrationPointWithoutConfiguration = new Data.IntegrationPoint { SecuredConfiguration = string.Empty };
		}

		[Test]
		public void ShouldThrowWhenRetrievingIntegrationPointsThrow()
		{
			_integrationPointForSourceService.GetAllForSourceProvider(Arg.Any<string>()).Throws<TimeoutException>();

			Assert.Throws<TimeoutException>(_command.Execute);
		}

		[Test]
		public void ShouldNotThrowWhenNoIntegrationPointsAreReturned()
		{
			_integrationPointForSourceService.GetAllForSourceProvider(Arg.Any<string>()).Returns(new List<Data.IntegrationPoint>());

			_command.Execute();
		}

		[Test]
		public void ShouldNotRemoveSecuredConfigurationWhenSecureStoreIsNotUsed()
		{
			_integrationPointForSourceService.GetAllForSourceProvider(Arg.Any<string>()).Returns(new List<Data.IntegrationPoint> { _integrationPointWithoutConfiguration });

			_command.Execute();

			_removeSecuredConfigurationService.DidNotReceiveWithAnyArgs().RemoveSecuredConfiguration(null);
		}

		[Test]
		public void ShouldNotUpdateRecordWhenNoChangesAreMade()
		{
			_removeSecuredConfigurationService.RemoveSecuredConfiguration(null).ReturnsForAnyArgs(false);

			_integrationPointForSourceService.GetAllForSourceProvider(Arg.Any<string>()).Returns(new List<Data.IntegrationPoint> { _integrationPointWithConfiguration });

			_command.Execute();

			_integrationPointService.DidNotReceiveWithAnyArgs().SaveIntegration(null);
		}


		[Test]
		public void ShouldUpdateRecordWhenFieldsWereUpdated()
		{
			_removeSecuredConfigurationService.RemoveSecuredConfiguration(null).ReturnsForAnyArgs(true);

			_integrationPointForSourceService.GetAllForSourceProvider(Arg.Any<string>()).Returns(new List<Data.IntegrationPoint> { _integrationPointWithConfiguration });

			_command.Execute();

			_integrationPointService.ReceivedWithAnyArgs(1).SaveIntegration(null);
		}

		[Test]
		public void ShouldProcessAllValidIntegrationPoints()
		{
			_removeSecuredConfigurationService.RemoveSecuredConfiguration(null).ReturnsForAnyArgs(true);

			_integrationPointForSourceService.GetAllForSourceProvider(Arg.Any<string>()).Returns(new List<Data.IntegrationPoint>()
			{
				_integrationPointWithConfiguration,
				_integrationPointWithConfiguration,
				_integrationPointWithoutConfiguration,
				_integrationPointWithConfiguration,
				_integrationPointWithoutConfiguration
			});

			_command.Execute();

			_integrationPointService.ReceivedWithAnyArgs(3).SaveIntegration(null);
		}
	}
}