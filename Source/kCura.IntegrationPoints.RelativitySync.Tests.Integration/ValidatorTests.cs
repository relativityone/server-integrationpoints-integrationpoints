using System;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class ValidatorTests : RelativityProviderTemplate
	{
		private ValidationConfigurationStub _config;

		public ValidatorTests() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();
			_config = new ValidationConfigurationStub();
		}

		[Test]
		public async Task ItShouldAlwaysReturnTrueForCanExecute()
		{
			Mock<IValidationExecutorFactory> factory = new Mock<IValidationExecutorFactory>();
			Validator validator = new Validator(Container, factory.Object);
			bool canExecute = await validator.CanExecuteAsync(_config, CancellationToken.None).ConfigureAwait(false);
			Assert.IsTrue(canExecute);
		}

		[Test]
		public async Task ItShouldPassValidationOnValidIntegrationPointModel()
		{
			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay, "SomeName", "Append Only");
			CreateOrUpdateIntegrationPoint(integrationPointModel);

			Validator validator = new Validator(Container, new ValidationExecutorFactory(Container));
			await validator.ExecuteAsync(_config, CancellationToken.None).ConfigureAwait(false);

			Assert.Pass();
		}
	}
}