using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture]
    internal class FederatedInstanceRsapiHandlerTests : FederatedInstanceRsapiHandler
    {
        private static ITokenProvider tokenProvider => Substitute.For<ITokenProvider>();
        private static OAuthClientDto oAuthClientDto => new OAuthClientDto();
        private static Uri instanceUri => new Uri("http://federated.instance.com/Relativity");

        [Test]
        public void ItShouldThrowExceptionWhenTokenProviderDoesntProvideToken()
        {
            // act  & assert

            Assert.Throws<IntegrationPointsException>(() => GetAuthenticationType(ExecutionIdentity.CurrentUser));
        }

        public FederatedInstanceRsapiHandlerTests() : base(FederatedInstanceRsapiHandlerTests.instanceUri,
            FederatedInstanceRsapiHandlerTests.oAuthClientDto, FederatedInstanceRsapiHandlerTests.tokenProvider)
        {
        }

        public FederatedInstanceRsapiHandlerTests(Uri instanceUri, OAuthClientDto oAuthClientDto,
            ITokenProvider tokenProvider) : base(FederatedInstanceRsapiHandlerTests.instanceUri, FederatedInstanceRsapiHandlerTests.oAuthClientDto, FederatedInstanceRsapiHandlerTests.tokenProvider)
        {
        }
    }
}
