using System.Collections.Generic;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.Security;
using Moq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class UpdateFtpConfigurationCommandTests : UpdateConfigurationCommandTestsBase
	{
		private Mock<IEncryptionManager> _encryptionManagerFake;
		private Mock<ISplitJsonObjectService> _splitServiceFake;

		private UpdateFtpConfigurationCommand _sut;

		protected override List<string> Names => new List<string>() { "Secured Configuration", "Source Configuration" };

		private const string _USERNAME_FIELD_NAME = "username";
		private const string _PASSWORD_FIELD_NAME = "password";

		public override void SetUp()
		{
			base.SetUp();

			_encryptionManagerFake = new Mock<IEncryptionManager>();
			_splitServiceFake = new Mock<ISplitJsonObjectService>();

			_sut = new UpdateFtpConfigurationCommand(EHHelperFake.Object, RelativityObjectManagerMock.Object, 
				_encryptionManagerFake.Object, _splitServiceFake.Object);
		}

		[Test]
		public void Execute_ShouldNotProcess_WhenSecuredConfigurationIsNotEmpty()
		{
			// Arrange
			RelativityObjectSlim objectSlim = PrepareObject(securedConfiguration: "Not Empty Config", sourceConfiguration: "");
			SetupRead(objectSlim);

			// Act
			_sut.Execute();

			// Assert
			ShouldNotBeUpdated();
		}


		[Test]
		public void Execute_ShouldProcess()
		{
			// Arrange
			const string decryptedConfiguration = @"{decryptedConfiguration}";
			var splittedConfiguration = new SplittedJsonObject
			{
				JsonWithExtractedProperties = "{SerializedSecuredConfiguration}",
				JsonWithoutExtractedProperties = "{SerializedSettings}"
			};

			RelativityObjectSlim objectSlim = PrepareObject(securedConfiguration: "", sourceConfiguration: "Some Configuration");
			RelativityObjectSlim objectSlimExpected = PrepareObject(securedConfiguration: splittedConfiguration.JsonWithExtractedProperties,
				sourceConfiguration: splittedConfiguration.JsonWithoutExtractedProperties);

			SetupRead(objectSlim);

			_encryptionManagerFake.Setup(x => x.Decrypt(It.IsAny<string>())).Returns(decryptedConfiguration);
			_splitServiceFake.Setup(x => x.Split(decryptedConfiguration, _USERNAME_FIELD_NAME, _PASSWORD_FIELD_NAME))
				.Returns(splittedConfiguration);

			// Act
			_sut.Execute();

			// Assert
			ShouldBeUpdated(objectSlimExpected);
		}

		[Test]
		public void Execute_ShouldNotProcess_WhenSplittingReturnsNull()
		{
			// Arrange
			const string decryptedConfiguration = @"{decryptedConfiguration}";

			RelativityObjectSlim objectSlim = PrepareObject(securedConfiguration: "", sourceConfiguration: "Some Configuration");
			SetupRead(objectSlim);

			_encryptionManagerFake.Setup(x => x.Decrypt(It.IsAny<string>())).Returns(decryptedConfiguration);

			_splitServiceFake.Setup(x => x.Split(decryptedConfiguration, _USERNAME_FIELD_NAME, _PASSWORD_FIELD_NAME))
				.Returns<SplittedJsonObject>(null);

			// Act
			_sut.Execute();

			// Assert
			ShouldNotBeUpdated();
		}

		private RelativityObjectSlim PrepareObject(string securedConfiguration, string sourceConfiguration)
		{
			return new RelativityObjectSlim()
			{
				ArtifactID = 1,
				Values = new List<object>()
				{
					securedConfiguration,
					sourceConfiguration
				}
			};
		}
	}
}