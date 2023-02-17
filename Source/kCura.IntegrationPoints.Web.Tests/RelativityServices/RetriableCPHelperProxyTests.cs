using System;
using FluentAssertions;
using kCura.IntegrationPoints.Web.RelativityServices;
using kCura.IntegrationPoints.Web.RelativityServices.Exceptions;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.RelativityServices
{
    [TestFixture, Category("Unit")]
    public class RetriableCPHelperProxyTests
    {
        [Test]
        public void ShouldThrowIfBaseCpHelperOfTypeRetriableCpHelper()
        {
            // arrange
            ICPHelper cpHelper = Substitute.For<ICPHelper>();
            RetriableCPHelperProxy baseCpHelper = Substitute.For<RetriableCPHelperProxy>(cpHelper);

            // act
            Action act = () => new RetriableCPHelperProxy(baseCpHelper);

            // assert
            act.ShouldThrow<BaseCpHelperCannotBeTypeOfRetriableCpHelperException>();
        }

        [Test]
        public void ShouldNotRetryIfGetActiveCaseIdReturnsNotZero()
        {
            // arrange
            const int workspaceId = 1019723;

            ICPHelper baseCpHelper = Substitute.For<ICPHelper>();
            baseCpHelper.GetActiveCaseID().Returns(workspaceId);

            var proxy = new RetriableCPHelperProxy(baseCpHelper);

            // act
            int result = proxy.GetActiveCaseID();

            // assert
            result.Should().Be(workspaceId);
            baseCpHelper.Received(1).GetActiveCaseID();
        }

        [Test]
        public void ShouldRetryIfFirstGetActiveCaseIdCallReturnsZero()
        {
            // arrange
            const int workspaceId = 1019723;

            ICPHelper baseCpHelper = Substitute.For<ICPHelper>();
            baseCpHelper.GetActiveCaseID().Returns(0, workspaceId);

            var proxy = new RetriableCPHelperProxy(baseCpHelper);

            // act
            int result = proxy.GetActiveCaseID();

            // assert
            result.Should().Be(workspaceId);
            baseCpHelper.Received(2).GetActiveCaseID();
        }

        [Test]
        public void ShouldRetryNotMoreThanTwoTimesIftGetActiveCaseIdCallsReturnZero()
        {
            // arrange
            ICPHelper baseCpHelper = Substitute.For<ICPHelper>();
            baseCpHelper.GetActiveCaseID().Returns(0, 0, 0, 0, 0, 0);

            var proxy = new RetriableCPHelperProxy(baseCpHelper);

            // act
            int result = proxy.GetActiveCaseID();

            // assert
            result.Should().Be(0);
            baseCpHelper.Received(3).GetActiveCaseID();
        }
    }
}
