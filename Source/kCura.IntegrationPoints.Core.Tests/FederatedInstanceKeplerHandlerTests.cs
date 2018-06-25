using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture]
    internal class FederatedInstanceKeplerHandlerTests : FederatedInstanceKeplerHandler
    {
        private static ITokenProvider tokenProvider => Substitute.For<ITokenProvider>();
        private static OAuthClientDto oAuthClientDto => new OAuthClientDto();
        private static Uri instanceUri => new Uri("http://federated.instance.com/Relativity");

        [Test]
        public void ItShouldThrowExceptionWhenTokenProviderDoesntProvideToken()
        {
            // act  & assert

            Assert.Throws<IntegrationPointsException>(() => GetKeplerCredentials(ExecutionIdentity.CurrentUser));
        }

        public FederatedInstanceKeplerHandlerTests() : base (FederatedInstanceKeplerHandlerTests.instanceUri, FederatedInstanceKeplerHandlerTests.oAuthClientDto, FederatedInstanceKeplerHandlerTests.tokenProvider)
        {
            
        }
        public FederatedInstanceKeplerHandlerTests(Uri instanceUri, OAuthClientDto oAuthClientDto, ITokenProvider tokenProvider) : base(FederatedInstanceKeplerHandlerTests.instanceUri, FederatedInstanceKeplerHandlerTests.oAuthClientDto, FederatedInstanceKeplerHandlerTests.tokenProvider)
        {
        }
    }
}
