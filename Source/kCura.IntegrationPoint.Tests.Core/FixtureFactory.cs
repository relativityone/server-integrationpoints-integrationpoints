using AutoFixture;
using AutoFixture.AutoMoq;

namespace kCura.IntegrationPoint.Tests.Core
{
    internal class FixtureFactory
    {
        public static IFixture Create()
        {
            return new Fixture().Customize(new AutoMoqCustomization() { ConfigureMembers = true });
        }
    }
}
