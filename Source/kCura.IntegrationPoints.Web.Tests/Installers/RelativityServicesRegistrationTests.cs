using Castle.Core;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Web.Installers;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Installers
{
    [TestFixture, Category("Unit")]
    public class RelativityServicesRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut.AddRelativityServices();
        }

        [Test]
        public void IStringSanitizer_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IStringSanitizer>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
        }

        [Test]
        public void IAPILog_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IAPILog>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
        }

        [Test]
        public void IAPILog_ShouldBeRegisteredWithProperName()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IAPILog>()
                .Which.Should().BeRegisteredWithName("ApiLogFromWeb");
        }
    }
}
