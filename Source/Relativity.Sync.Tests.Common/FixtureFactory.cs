using AutoFixture;
using AutoFixture.AutoMoq;

namespace Relativity.Sync.Tests.Common
{
    internal class FixtureFactory
    {
        public static IFixture Create()
        {
            return new Fixture().Customize(new AutoMoqCustomization() { ConfigureMembers = true });
        }
    }
}
