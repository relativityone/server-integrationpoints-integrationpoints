using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands.Helpers
{
	[TestFixture]
	public class TenantForSecretStoreCreationValidatorTests : TestBase
	{
		private TenantForSecretStoreCreationValidator _instance;
		private ISecretManager _secretManager;
		private ISecretCatalog _secretCatalog;

		public override void SetUp()
		{
			_secretManager = Substitute.For<ISecretManager>();
			_secretCatalog = Substitute.For<ISecretCatalog>();

			var workspaceId = 107526;
			var helper = Substitute.For<IEHHelper>();
			helper.GetActiveCaseID().Returns(workspaceId);
			var context = new EHContext
			{
				Helper = helper
			};
			var secretManagerFactory = Substitute.For<ISecretManagerFactory>();
			secretManagerFactory.Create(workspaceId).Returns(_secretManager);
			var secretCatalogFactory = Substitute.For<ISecretCatalogFactory>();
			secretCatalogFactory.Create(workspaceId).Returns(_secretCatalog);

			_instance = new TenantForSecretStoreCreationValidator(context, secretManagerFactory, secretCatalogFactory);
		}

		[Test]
		public void ItShouldValidateByCreatingAndRevokingSecret()
		{
			_secretCatalog.WriteSecret(Arg.Any<SecretRef>(), Arg.Any<Dictionary<string, string>>()).Returns(true);

			// ACT
			var result = _instance.Validate();

			// ASSERT
			Assert.That(result, Is.True);
			_secretCatalog.Received(1).WriteSecret(Arg.Any<SecretRef>(), Arg.Any<Dictionary<string, string>>());
			_secretCatalog.Received(1).RevokeSecret(Arg.Any<SecretRef>());
		}

		[Test]
		public void ItShouldReturnFalseForCreatingFailure()
		{
			_secretCatalog.WriteSecret(Arg.Any<SecretRef>(), Arg.Any<Dictionary<string, string>>()).Returns(false);

			// ACT
			var result = _instance.Validate();

			// ASSERT
			Assert.That(result, Is.False);
		}

		[Test]
		public void ItShouldReturnFalseForCreatingException()
		{
			_secretCatalog.When(x => x.WriteSecret(Arg.Any<SecretRef>(), Arg.Any<Dictionary<string, string>>())).Throw<Exception>();

			// ACT
			var result = _instance.Validate();

			// ASSERT
			Assert.That(result, Is.False);
		}

		[Test]
		public void ItShouldIgnoreRevokingFailure()
		{
			_secretCatalog.WriteSecret(Arg.Any<SecretRef>(), Arg.Any<Dictionary<string, string>>()).Returns(true);
			_secretCatalog.When(x => x.RevokeSecret(Arg.Any<SecretRef>())).Throw<Exception>();

			// ACT
			var result = _instance.Validate();

			// ASSERT
			Assert.That(result, Is.True);
		}
	}
}