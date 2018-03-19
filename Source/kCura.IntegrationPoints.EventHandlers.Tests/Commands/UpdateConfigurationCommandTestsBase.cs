using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.EventHandlers.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public abstract class UpdateConfigurationCommandTestsBase : TestBase
	{
		protected abstract string ExpectedProviderType { get; }

		protected IIntegrationPointForSourceService IntegrationPointForSourceService { get; set; }
		protected IIntegrationPointService IntegrationPointService { get; set; }
		protected Data.IntegrationPoint IntegrationPointWithoutSecuredConfiguration { get; set; }
		protected Data.IntegrationPoint IntegrationPointWithSecuredConfiguration { get; set; }

		protected IEHCommand Command { get; set; }

		public override void SetUp()
		{
			IntegrationPointForSourceService = Substitute.For<IIntegrationPointForSourceService>();
			IntegrationPointService = Substitute.For<IIntegrationPointService>();

			IntegrationPointWithoutSecuredConfiguration = new Data.IntegrationPoint
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
				SecuredConfiguration = "",
				PromoteEligible = true,
				ScheduleRule = ""
			};

			IntegrationPointWithSecuredConfiguration =
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
		}

		protected virtual void ShouldProcessAllValidIntegrationPoints(int expectedNumberOfUpdates)
		{
			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Is(ExpectedProviderType))
				.Returns(new List<Data.IntegrationPoint>
				{
					IntegrationPointWithoutSecuredConfiguration,
					IntegrationPointWithoutSecuredConfiguration,
					IntegrationPointWithSecuredConfiguration,
					IntegrationPointWithoutSecuredConfiguration,
					IntegrationPointWithSecuredConfiguration
				});

			Command.Execute();

			IntegrationPointService.ReceivedWithAnyArgs(expectedNumberOfUpdates).SaveIntegration(null);
		}

		[Test]
		public void ShouldThrowWhenRetrievingIntegrationPointsThrow()
		{
			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Is(ExpectedProviderType)).Throws<TimeoutException>();

			Assert.Throws<TimeoutException>(Command.Execute);
		}

		[Test]
		public void ShouldNotThrowWhenNoIntegrationPointsAreReturned()
		{
			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Is(ExpectedProviderType)).Returns(new List<Data.IntegrationPoint>());

			Command.Execute();
		}

		[Test]
		public void ShouldQueryForCorrectProvider()
		{
			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Any<string>()).Returns(new List<Data.IntegrationPoint>());

			Command.Execute();

			IntegrationPointForSourceService.Received(1).GetAllForSourceProvider(Arg.Is(ExpectedProviderType));
		}

		[Test]
		public abstract void ShouldProcessAllValidIntegrationPoints();
	}
}