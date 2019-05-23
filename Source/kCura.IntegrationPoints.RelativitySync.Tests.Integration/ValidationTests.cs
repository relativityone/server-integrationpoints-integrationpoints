﻿using System;
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
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class ValidationTests : RelativityProviderTemplate
	{
		private ValidationConfigurationStub _config;
		private Mock<IExtendedJob> _extendedJobMock;

		public ValidationTests() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
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
			_config = new ValidationConfigurationStub();
		}

		[IdentifiedTest("4a97b756-a5ff-45c2-8f3c-8c267a156fb6")]
		public async Task ItShouldAlwaysReturnTrueForCanExecute()
		{
			Mock<IValidationExecutorFactory> factory = new Mock<IValidationExecutorFactory>();
			Mock<IRdoRepository> rdoRepository = new Mock<IRdoRepository>();

			Validation validator = new Validation(Container, factory.Object, rdoRepository.Object);
			bool canExecute = await validator.CanExecuteAsync(_config, CancellationToken.None).ConfigureAwait(false);
			Assert.IsTrue(canExecute);
		}

		[IdentifiedTest("bd98ed86-4b74-437d-8a00-eebb53797314")]
		public async Task ItShouldPassValidationOnValidIntegrationPointModel()
		{
			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay, "SomeName", "Append Only");
			CreateOrUpdateIntegrationPoint(integrationPointModel);

			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();
			int integrationPointArtifactId = service.SaveIntegration(integrationPointModel);
			Data.IntegrationPoint integrationPoint = service.ReadIntegrationPoint(integrationPointArtifactId);

			_extendedJobMock.SetupGet(x => x.IntegrationPointModel).Returns(integrationPoint);

			Validation validator = new Validation(Container, new ValidationExecutorFactory(Container), new RdoRepository(Container));
			await validator.ExecuteAsync(_config, CancellationToken.None).ConfigureAwait(false);

			Assert.Pass();
		}
	}
}