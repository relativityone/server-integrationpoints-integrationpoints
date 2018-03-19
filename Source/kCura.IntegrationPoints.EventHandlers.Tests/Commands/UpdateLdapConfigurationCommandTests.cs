using System.Collections.Generic;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.Security;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class UpdateLdapConfigurationCommandTests : UpdateConfigurationCommandTestsBase
	{
		private IEncryptionManager _encryptionManager;
		private ISplitJsonObjectService _splitService;

		private const string _USERNAME_FIELD_NAME = "userName";
		private const string _PASSWORD_FIELD_NAME = "password";

		protected override string ExpectedProviderType => Constants.IntegrationPoints.SourceProviders.LDAP;

		public override void SetUp()
		{
			base.SetUp();

			_encryptionManager = Substitute.For<IEncryptionManager>();
			_splitService = Substitute.For<ISplitJsonObjectService>();

			Command = new UpdateLdapConfigurationCommand(IntegrationPointForSourceService, IntegrationPointService, _encryptionManager, _splitService);
		}

		[Test]
		public override void ShouldProcessAllValidIntegrationPoints()
		{
			_splitService.Split(Arg.Any<string>(), _USERNAME_FIELD_NAME, _PASSWORD_FIELD_NAME).Returns(new SplittedJsonObject());

			ShouldProcessAllValidIntegrationPoints(3);
		}

		[Test]
		public void ShouldProperlyUpdateConfiguration()
		{
			const string decryptedConfiguration = @"{decryptedConfiguration}";
			var splittedConfiguration = new SplittedJsonObject
			{
				JsonWithExtractedProperties = "{SerializedSecuredConfiguration}",
				JsonWithoutExtractedProperties = "{SerializedSettings}"
			};

			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Is(ExpectedProviderType))
				.Returns(new List<Data.IntegrationPoint> { IntegrationPointWithoutSecuredConfiguration });

			_encryptionManager.Decrypt(Arg.Any<string>()).Returns(decryptedConfiguration);

			_splitService.Split(Arg.Is(decryptedConfiguration), _USERNAME_FIELD_NAME, _PASSWORD_FIELD_NAME).Returns(splittedConfiguration);

			Command.Execute();

			IntegrationPointService.Received(1).SaveIntegration(Arg.Is<IntegrationPointModel>(model =>
				string.Equals(model.SourceConfiguration, @"{SerializedSettings}") &&
				string.Equals(model.SecuredConfiguration, @"{SerializedSecuredConfiguration}"))
			);
		}

		[Test]
		public void ShouldNotUpdateWhenSplittingReturnsNull()
		{
			const string decryptedConfiguration = @"{decryptedConfiguration}";

			IntegrationPointForSourceService.GetAllForSourceProvider(Arg.Is(ExpectedProviderType))
				.Returns(new List<Data.IntegrationPoint> { IntegrationPointWithoutSecuredConfiguration });

			_encryptionManager.Decrypt(Arg.Any<string>()).Returns(decryptedConfiguration);

			_splitService.Split(Arg.Is(decryptedConfiguration), _USERNAME_FIELD_NAME, _PASSWORD_FIELD_NAME).ReturnsNull();

			Command.Execute();

			IntegrationPointService.DidNotReceiveWithAnyArgs().SaveIntegration(null);
		}
	}
}