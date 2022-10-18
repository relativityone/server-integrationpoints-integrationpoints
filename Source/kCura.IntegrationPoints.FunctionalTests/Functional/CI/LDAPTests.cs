using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI]
    [TestType.MainFlow]
    public class LDAPTests : TestsBase
    {
        private readonly ImportLDAPTestImplementation _testsImplementation;

        public LDAPTests()
            : base(nameof(LDAPTests))
        {
            _testsImplementation = new ImportLDAPTestImplementation(this);
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();

            _testsImplementation.OnSetUpFixture();
        }

        [Test]
        [Ignore("REL-753202")]
        public void LoadFromLDAP_GoldFlow()
        {
            _testsImplementation.ImportFromLDAPGoldFlow();
        }
    }
}
