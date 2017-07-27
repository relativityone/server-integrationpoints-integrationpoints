using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	[TestFixture]
	public class CreateTenantIdForSecretStoreCommandTests : TestBase
	{
		private CreateTenantIdForSecretStoreCommand _instance;
		private ICreateTenantIdForSecretStore _createTenantIdForSecretStore;
		private ITenantForSecretStoreCreationValidator _validator;

		public override void SetUp()
		{
			_createTenantIdForSecretStore = Substitute.For<ICreateTenantIdForSecretStore>();
			_validator = Substitute.For<ITenantForSecretStoreCreationValidator>();
			_instance = new CreateTenantIdForSecretStoreCommand(_createTenantIdForSecretStore, _validator);
		}

		[Test]
		public void ItShouldCreateAndValidateTenant()
		{
			_validator.Validate().Returns(true);

			// ACT
			_instance.Execute();

			// ASSERT
			_createTenantIdForSecretStore.Received(1).Create();
			_validator.Received(1).Validate();
		}

		[Test]
		public void ItShouldThrowExceptionForValidationFailure()
		{
			_validator.Validate().Returns(false);

			// ACT
			Assert.That(() => _instance.Execute(), Throws.TypeOf<CommandExecutionException>());
		}
	}
}