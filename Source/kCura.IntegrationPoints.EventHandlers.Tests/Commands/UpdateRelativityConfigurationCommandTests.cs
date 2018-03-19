using System.Collections.Generic;
using kCura.IntegrationPoints.EventHandlers.Commands;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class UpdateRelativityConfigurationCommandTests : UpdateConfigurationCommandTestsBase
	{
		private IRemoveSecuredConfigurationFromIntegrationPointService _removeSecuredConfigurationService;

		protected override string ExpectedProviderType => Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY;

		public override void SetUp()
		{
			base.SetUp();

			_removeSecuredConfigurationService = Substitute.For<IRemoveSecuredConfigurationFromIntegrationPointService>();

			Command = new UpdateRelativityConfigurationCommand(IntegrationPointForSourceService, IntegrationPointService, _removeSecuredConfigurationService);			
		}

		[Test]
		public override void ShouldProcessAllValidIntegrationPoints()
		{
			_removeSecuredConfigurationService.RemoveSecuredConfiguration(null).ReturnsForAnyArgs(true);

			ShouldProcessAllValidIntegrationPoints(2);
		}

		[Test]
		public void ShouldNotRemoveSecuredConfigurationWhenSecureStoreIsNotUsed()
		{
			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Is(ExpectedProviderType))
				.Returns(new List<Data.IntegrationPoint> { IntegrationPointWithoutSecuredConfiguration });

			Command.Execute();

			_removeSecuredConfigurationService.DidNotReceiveWithAnyArgs().RemoveSecuredConfiguration(null);
		}

		[Test]
		public void ShouldNotUpdateRecordWhenNoChangesAreMade()
		{
			_removeSecuredConfigurationService.RemoveSecuredConfiguration(null).ReturnsForAnyArgs(false);

			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Is(ExpectedProviderType))
				.Returns(new List<Data.IntegrationPoint> { IntegrationPointWithSecuredConfiguration });

			Command.Execute();

			IntegrationPointService.DidNotReceiveWithAnyArgs().SaveIntegration(null);
		}


		[Test]
		public void ShouldUpdateRecordWhenFieldsWereUpdated()
		{
			_removeSecuredConfigurationService.RemoveSecuredConfiguration(null).ReturnsForAnyArgs(true);

			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Is(ExpectedProviderType))
				.Returns(new List<Data.IntegrationPoint> { IntegrationPointWithSecuredConfiguration });

			Command.Execute();

			_removeSecuredConfigurationService.ReceivedWithAnyArgs(1).RemoveSecuredConfiguration(null);

			IntegrationPointService.ReceivedWithAnyArgs(1).SaveIntegration(null);
		}
		
	}
}