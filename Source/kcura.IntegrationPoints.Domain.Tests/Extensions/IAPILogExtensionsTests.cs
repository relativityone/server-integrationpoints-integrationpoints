using System;
using kCura.IntegrationPoints.Domain.Extensions;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Domain.Tests.Extensions
{
    [TestFixture, Category("Unit")]
    public class IAPILogExtensionsTests
    {
        private const string _RIP_PREFIX = "RIP.";

        [Test]
        public void ItShouldPushPublicPropertiesWithProperValue()
        {
            var context = new TestContextWithNonPublicProperties
            {
                Id = 432,
                PublicName = "test"
            };

            var logger = Substitute.For<IAPILog>();
            logger.LogContextPushProperties(context);

            logger.Received().LogContextPushProperty($"{_RIP_PREFIX}{nameof(context.Id)}", context.Id.ToString());
            logger.Received().LogContextPushProperty($"{_RIP_PREFIX}{nameof(context.PublicName)}", context.PublicName);
        }

        [Test]
        public void ItShouldNotPushNonPublicProperties()
        {
            var context = new TestContextWithNonPublicProperties
            {
                Id = 432,
                PublicName = "test"
            };

            var logger = Substitute.For<IAPILog>();
            logger.LogContextPushProperties(context);

            logger.DidNotReceive().LogContextPushProperty("Name", Arg.Any<object>());
            logger.DidNotReceive().LogContextPushProperty("Date", Arg.Any<object>());
        }

        [Test]
        public void ItShouldNotPushPublicFields()
        {
            var context = new TestContextWithPublicField
            {
                Id = 432,
                Name = "ShouldNotBePushed"
            };

            var logger = Substitute.For<IAPILog>();
            logger.LogContextPushProperties(context);

            logger.DidNotReceive().LogContextPushProperty(nameof(context.Name), Arg.Any<object>());
        }

        [Test]
        public void ItShouldCallDisposeOnInnerDisposableObjectsInProperOrder()
        {
            // ARRANGE
            var context = new TestContextWithNonPublicProperties
            {
                Id = 432,
                PublicName = "ShouldBePushed"
            };

            var logger = Substitute.For<IAPILog>();
            var disposableForId = Substitute.For<IDisposable>();
            var disposableForPublicName = Substitute.For<IDisposable>();
            logger.LogContextPushProperty($"{_RIP_PREFIX}{nameof(context.Id)}", Arg.Any<object>()).Returns(disposableForId);
            logger.LogContextPushProperty($"{_RIP_PREFIX}{nameof(context.PublicName)}", Arg.Any<object>()).Returns(disposableForPublicName);

            // ACT
            using (logger.LogContextPushProperties(context))
            {
            }

            // ASSERT
            bool shouldVerifyInCatch = true;
            try // expected order of Dispose calls depends on order of calls to LogContextPushProperty
            {
                Received.InOrder(() =>
                {
                    logger.LogContextPushProperty($"{_RIP_PREFIX}Id", Arg.Any<object>());
                    logger.LogContextPushProperty($"{_RIP_PREFIX}PublicName", Arg.Any<object>());
                });

                shouldVerifyInCatch = false;
                Received.InOrder(() =>
                {
                    disposableForPublicName.Dispose();
                    disposableForId.Dispose();
                });
            }
            catch
            {
                if (!shouldVerifyInCatch)
                {
                    throw;
                }
                Received.InOrder(() =>
                {
                    disposableForId.Dispose();
                    disposableForPublicName.Dispose();
                });
            }
        }

        private class TestContextWithNonPublicProperties
        {
            public int Id { get; set; }
            public string PublicName { get; set; }
            protected string Name { get; set; }
            private DateTime Date { get; set; }

            public TestContextWithNonPublicProperties()
            {
                Name = "Test";
                Date = DateTime.Now;

            }
        }

        private class TestContextWithPublicField
        {
            public int Id { get; set; }
            public string Name;
        }
    }
}
