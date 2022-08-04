using FluentAssertions;
using kCura.IntegrationPoints.Common;
using NUnit.Framework;
using System;
using kCura.IntegrationPoints.Common.Handlers;

namespace kCura.IntegrationPoints.Data.Tests
{
    [TestFixture, Category("Unit")]
    public class RetryHandlerFactoryTests
    {
        [Test]
        public void CreateWithoutParametersShouldWorkWithNullLoger()
        {
            ShouldWorkWithNull(x => x.Create());
        }

        [Test]
        public void CreateWithParametersShouldWorkWithNullLoger()
        {
            ShouldWorkWithNull(x => x.Create(1, 4));
        }

        private static void ShouldWorkWithNull(Func<RetryHandlerFactory, IRetryHandler> createMethod)
        {
            // arrange
            var factory = new RetryHandlerFactory(null);

            // act
            IRetryHandler handler = createMethod(factory);

            // assert
            handler.Should().NotBeNull();
        }
    }
}
