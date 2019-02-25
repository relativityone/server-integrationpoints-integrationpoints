using System;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class PermissionsCheckTests : RelativityProviderTemplate
	{
		private PermissionsCheckConfigurationStub _config;

		public PermissionsCheckTests() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();
			_config = new PermissionsCheckConfigurationStub();
		}

		[Test]
		public async Task ItShouldAlwaysReturnTrueForCanExecute()
		{
			Mock<IValidationExecutorFactory> factory = new Mock<IValidationExecutorFactory>();
			Mock<IRdoRepository> rdoRepository = new Mock<IRdoRepository>();
			PermissionsCheck validator = new PermissionsCheck(Container, factory.Object, rdoRepository.Object);
			bool canExecute = await validator.CanExecuteAsync(_config, CancellationToken.None).ConfigureAwait(false);
			Assert.IsTrue(canExecute);
		}

		[Test]
		public async Task ItShouldPassValidationOnValidIntegrationPointModel()
		{
			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay, "SomeName", "Append Only");
			CreateOrUpdateIntegrationPoint(integrationPointModel);
			_config.ExecutingUserId = ADMIN_USER_ID;

			PermissionsCheck validator = new PermissionsCheck(Container, new ValidationExecutorFactory(Container), new RdoRepository(Container));
			await validator.ExecuteAsync(_config, CancellationToken.None).ConfigureAwait(false);

			Assert.Pass();
		}
	}
}
