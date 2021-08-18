using System.Threading.Tasks;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestType.UI, TestType.MainFlow]
	public class LDAPTest : TestsBase
	{
		private readonly ImportLDAPTestImplementation _testsImplementation;

		public LDAPTest()
			: base(nameof(LDAPTest))
		{
			_testsImplementation = new ImportLDAPTestImplementation(this);
		}

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			_testsImplementation.OnSetUpFixture();
		}

		[IdentifiedTest("09c54ba0-04d9-4f6e-9c46-0075612582fa")]
		public void LoadFromLDAP_GoldFlow()
		{
			_testsImplementation.ImportFromLDAPGoldFlow();
		}
	}
}