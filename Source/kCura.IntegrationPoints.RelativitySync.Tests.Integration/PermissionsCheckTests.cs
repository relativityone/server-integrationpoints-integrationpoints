using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class PermissionsCheckTests : RelativityProviderTemplate
	{
		private PermissionsCheckConfigurationStub _config;
		private Mock<IExtendedJob> _extendedJobMock;

		public PermissionsCheckTests() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_extendedJobMock = new Mock<IExtendedJob>();
			Container.Register(Component.For<IExtendedJob>().Instance(_extendedJobMock.Object));
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

			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();
			int integrationPointArtifactId = service.SaveIntegration(integrationPointModel);
			Data.IntegrationPoint integrationPoint = service.GetRdo(integrationPointArtifactId);

			_extendedJobMock.SetupGet(x => x.IntegrationPointModel).Returns(integrationPoint);

			PermissionsCheck validator = new PermissionsCheck(Container, new ValidationExecutorFactory(Container), new RdoRepository(Container));
			await validator.ExecuteAsync(_config, CancellationToken.None).ConfigureAwait(false);

			Assert.Pass();
		}
	}
}
