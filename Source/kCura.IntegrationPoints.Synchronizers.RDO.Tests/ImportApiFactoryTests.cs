using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
        private const string _federatedInstanceToken = "mnbvcxzlkjhgfdsapoiuytrewq-0987654321";
        private const string _localInstanceToken = "1234567890-asdfghjklzxcvbnmqwefertyuiop";
        private const string _clientId = "1234567";
        private const string _clientSecret = "16wg17w61gw21jsg2176";
        private const string _federatedInstanceAddress = "http://instance-in-the-kingdom-far-far-away.relativity.com";
        private string _usedToken { get; set; }
        private bool _mockIExtendedImportAPI = true;


        [SetUp]
        public void SetUp()
        {
            _authTokenGenerator.GetAuthToken()
                .Returns(_localInstanceToken);

            _externalTokenProvider.GetExternalSystemToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Uri>())
                .Returns(_federatedInstanceToken);

            _serializer.Deserialize<OAuthClientDto>(Arg.Any<string>())
                .Returns(new OAuthClientDto() { ClientId = _clientId, ClientSecret = _clientSecret });

            _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(Arg.Any<int>())
                .Returns(new FederatedInstanceDto() { InstanceUrl = _federatedInstanceAddress });




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
        public void ShouldUseExternalTokenProviderForFederatedInstance()
        {
            // arrange
            ClearTestCaseArtifacts();
            _mockIExtendedImportAPI = true;
            ImportSettings settings = new ImportSettings();
            settings.WebServiceURL = _localInstanceAddress;
            settings.FederatedInstanceArtifactId = _federatedInstanceArtifactId;
            // act 
            CreateExtendedImportAPIForSettings(settings);

            // assert
            _externalTokenProvider.Received(1).GetExternalSystemToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Uri>());
            Assert.AreEqual(_usedToken, _federatedInstanceToken);
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
            _externalTokenProvider.Received(0).GetExternalSystemToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Uri>());
            _authTokenGenerator.Received(0).GetAuthToken();
            Assert.AreEqual(_usedToken, _relativityPassword);
        }


        [Test]
        public void ShouldIgnoreFederatedInstanceIfCredentialsAreProvided()
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
            CreateExtendedImportAPIForSettings(settings);

            // assert
            _externalTokenProvider.Received(0).GetExternalSystemToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Uri>());
            _authTokenGenerator.Received(0).GetAuthToken();
            Assert.AreEqual(_usedToken, _relativityPassword);
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
            : base (Substitute.For<ITokenProvider>(), Substitute.For<IAuthTokenGenerator>(), Substitute.For<IFederatedInstanceManager>(), Substitute.For<IHelper>(), Substitute.For<ISystemEventLoggingService>(), Substitute.For<ISerializer>())
        {
            
        }
        
        private void ClearTestCaseArtifacts()
        {
            _usedToken = string.Empty;
            _externalTokenProvider.ClearReceivedCalls();
            _authTokenGenerator.ClearReceivedCalls();
        }
    }
}
