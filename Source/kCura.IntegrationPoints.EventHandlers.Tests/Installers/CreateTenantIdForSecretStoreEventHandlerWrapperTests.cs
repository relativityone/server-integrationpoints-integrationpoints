using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Installers
{
	public class CreateTenantIdForSecretStoreEventHandlerWrapperTests : TestBase
	{
		private IAPILog _logger;
		private ICreateTenantIdForSecretStore _createTenantIdForSecretStore;
		private CreateTenantIdForSecretStoreEventHandlerWrapper _installer;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_createTenantIdForSecretStore = Substitute.For<ICreateTenantIdForSecretStore>();

			var helper = Substitute.For<IEHHelper>();
			helper.GetLoggerFactory().GetLogger().ForContext<CreateTenantIdForSecretStoreEventHandlerWrapper>().Returns(_logger);
			helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<int>(Arg.Any<string>()).Returns(1);

			_installer = new CreateTenantIdForSecretStoreEventHandlerWrapper
			{
				CreateTenantIdForSecretStore = _createTenantIdForSecretStore,
				Helper = helper
			};
		}

		[Test]
		public void ItShouldCreateTenantId()
		{
			var response = _installer.Execute();

			Assert.That(response.Success, Is.True);
			Assert.That(response.Message, Is.EqualTo("SecretStore successfully initialized."));
			_createTenantIdForSecretStore.Received(1).Create();
		}

		[Test]
		public void ItShouldCatchErrorAndLogIt()
		{
			var expectedExceptionMessage = "error_message";
			var exception = new Exception(expectedExceptionMessage);
			_createTenantIdForSecretStore.When(x => x.Create()).Do(x => { throw exception; });

			var response = _installer.Execute();

			Assert.That(response.Success, Is.False);
			Assert.That(response.Message, Is.EqualTo("Failed to initialize SecretStore."));
			Assert.That(response.Exception.Message, Is.EqualTo(expectedExceptionMessage));
			_logger.Received(1).LogError(exception, "Failed to create TenantID in SecretStore.");
		}
	}
}