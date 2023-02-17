using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.Exporter;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
    [TestFixture, Category("Unit")]
    public class LazyExceptArtifactFieldDtoTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
        }

        [Test]
        public void DtoContainsException()
        {
            Exception exception = new Exception("");
            LazyExceptArtifactFieldDto field = new LazyExceptArtifactFieldDto(exception);

            Assert.Throws<Exception>(() => { object value = field.Value; });
        }

        [Test]
        public void DtoContainsNoException()
        {
            LazyExceptArtifactFieldDto field = new LazyExceptArtifactFieldDto(null);
            Assert.IsNull(field.Value);
        }

        [Test]
        public void DtoContainsValue()
        {
            const string expectedString = "String";
            LazyExceptArtifactFieldDto field = new LazyExceptArtifactFieldDto(null)
            {
                Value = expectedString
            };
            Assert.AreEqual(expectedString, field.Value);
        }

        [Test]
        public void DtoContainsValueAndException()
        {
            const string expectedString = "String";
            Exception exception = new Exception("lol");
            LazyExceptArtifactFieldDto field = new LazyExceptArtifactFieldDto(exception)
            {
                Value = expectedString
            };
            Assert.Throws<Exception>(() => { object value = field.Value; });
        }
    }
}
