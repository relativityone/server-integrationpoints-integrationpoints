using System;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
	[TestFixture]
	public class ImportApiFactoryTests : ImportApiFactory
	{
		private const string _localInstanceAddress = "http://instance-address.relativity.com/Relativity";
		private const string _relativityUserName = "relativity.admin";
		private const string _relativityPassword = "Password123";
		private const int _federatedInstanceArtifactId = 666;
		private const string _loginFailedExceptionMessage = "Login failed.";
		private const string _localInstanceToken = "1234567890-asdfghjklzxcvbnmqwefertyuiop";
		private string _usedToken { get; set; }
		private bool _mockIExtendedImportAPI = true;


		[SetUp]
		public void SetUp()
		{
			_authTokenGenerator.GetAuthToken()
				.Returns(_localInstanceToken);
		}

		[Test]
		public void ShouldUseAuthTokenGeneratorForSameInstance()
		{
			// arrange
			ClearTestCaseArtifacts();
			_mockIExtendedImportAPI = true;
			ImportSettings settings = new ImportSettings();
			settings.WebServiceURL = _localInstanceAddress;

			// act 
			CreateExtendedImportAPIForSettings(settings);

			// assert
			_authTokenGenerator.Received(1).GetAuthToken();
			Assert.AreEqual(_usedToken, _localInstanceToken);
		}

		[Test]
		public void ShouldThrowExceptionForFederatedInstance()
		{
			// arrange
			ClearTestCaseArtifacts();
			_mockIExtendedImportAPI = true;
			ImportSettings settings = new ImportSettings();
			settings.WebServiceURL = _localInstanceAddress;
			settings.FederatedInstanceArtifactId = _federatedInstanceArtifactId;

			// act 
			Action createImportApiAction = () => CreateExtendedImportAPIForSettings(settings);

			// assert
			try // TODO use Fluent Assertions
			{
				createImportApiAction();
				Assert.Fail();
			}
			catch (Exception)
			{
				Assert.Pass();
			}
		}

		[Test]
		public void ShouldUseUsernameAndPasswordIfProvided()
		{
			// arrange
			ClearTestCaseArtifacts();
			_mockIExtendedImportAPI = true;
			ImportSettings settings = new ImportSettings();
			settings.WebServiceURL = _localInstanceAddress;
			settings.RelativityUsername = _relativityUserName;
			settings.RelativityPassword = _relativityPassword;

			// act 
			CreateExtendedImportAPIForSettings(settings);

			// assert
			_authTokenGenerator.Received(0).GetAuthToken();
			Assert.AreEqual(_usedToken, _relativityPassword);
		}


		[Test]
		public void ShouldThrowInvalidOperationExceptionForInstanceToInstance()
		{
			// arrange
			ClearTestCaseArtifacts();
			_mockIExtendedImportAPI = true;
			ImportSettings settings = new ImportSettings();
			settings.WebServiceURL = _localInstanceAddress;
			settings.RelativityUsername = _relativityUserName;
			settings.RelativityPassword = _relativityPassword;
			settings.FederatedInstanceArtifactId = _federatedInstanceArtifactId;

			// act 
			TestDelegate createIAPIAction = () => CreateExtendedImportAPIForSettings(settings);

			// assert
			Assert.Throws<InvalidOperationException>(createIAPIAction);
		}


		[Test]
		public void ShouldThrowAuthorizationExceptionWhenIAPICannotLogIn()
		{
			// arrange
			ClearTestCaseArtifacts();
			_mockIExtendedImportAPI = false;
			ImportSettings settings = new ImportSettings();
			settings.WebServiceURL = _localInstanceAddress;

			// act & assert
			Assert.Throws<IntegrationPointsException>(() => GetImportAPI(settings));
		}

		protected override IExtendedImportAPI CreateExtendedImportAPI(string username, string token, string webServiceUrl)
		{
			_usedToken = token;
			if (_mockIExtendedImportAPI)
			{
				return Substitute.For<IExtendedImportAPI>();
			}
			else
			{
				throw new Exception(_loginFailedExceptionMessage);
			}
		}

		public ImportApiFactoryTests()
			: base(
				Substitute.For<IAuthTokenGenerator>(),
				Substitute.For<IAPILog>(),
				Substitute.For<ISystemEventLoggingService>())
		{
		}

		private void ClearTestCaseArtifacts()
		{
			_usedToken = string.Empty;
			_authTokenGenerator.ClearReceivedCalls();
		}
	}
}
