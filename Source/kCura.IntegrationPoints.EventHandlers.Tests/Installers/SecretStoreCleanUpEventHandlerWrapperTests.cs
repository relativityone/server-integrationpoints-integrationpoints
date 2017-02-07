using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Installers
{
	public class SecretStoreCleanUpEventHandlerWrapperTests : TestBase
	{
		private IAPILog _logger;
		private ISecretStoreCleanUp _secretStoreCleanUp;
		private SecretStoreCleanUpEventHandlerWrapper _cleanUpEventHandlerWrapper;


		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_secretStoreCleanUp = Substitute.For<ISecretStoreCleanUp>();

			var helper = Substitute.For<IEHHelper>();
			helper.GetLoggerFactory().GetLogger().ForContext<SecretStoreCleanUpEventHandlerWrapper>().Returns(_logger);
			_cleanUpEventHandlerWrapper = new SecretStoreCleanUpEventHandlerWrapper
			{
				Helper = helper,
				SecretStoreCleanUp = _secretStoreCleanUp
			};
		}

		[Test]
		public void ItShouldCleanUpSecretStore()
		{
			var response = _cleanUpEventHandlerWrapper.Execute();

			Assert.That(response.Success);
			Assert.That(response.Message, Is.EqualTo("Secret Store successfully cleaned up."));
			_secretStoreCleanUp.Received(1).CleanUpSecretStore();
		}

		[Test]
		public void ItShouldHandleException()
		{
			var expectedException = new Exception("error_message");
			_secretStoreCleanUp.When(x => x.CleanUpSecretStore()).Do(x => { throw expectedException; });

			var response = _cleanUpEventHandlerWrapper.Execute();

			Assert.That(response.Success, Is.False);
			Assert.That(response.Message, Is.EqualTo("Failed to clean up Secret Store."));
			Assert.That(response.Exception.Message, Is.EqualTo(expectedException.Message));
		}

		[Test]
		public void ItShouldLogException()
		{
			var expectedException = new Exception("error_message");
			_secretStoreCleanUp.When(x => x.CleanUpSecretStore()).Do(x => { throw expectedException; });

			_cleanUpEventHandlerWrapper.Execute();

			_logger.Received(1).LogError(expectedException, "Failed to clean up Secret Store.");
		}
	}
}