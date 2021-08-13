using System.Threading.Tasks;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;


namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestType.UI, TestType.MainFlow]
	public class LoadTest : TestsBase
	{
		private readonly ImportLDAPTestImplementation _testsImplementation;

		public LoadTest()
			: base(nameof(LoadTest))
		{
			_testsImplementation = new ImportLDAPTestImplementation(this);
		}

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			_testsImplementation.OnSetUpFixture();
		}

		protected override void OnTearDownFixture()
		{
			base.OnTearDownFixture();

			_testsImplementation.OnTearDownFixture();
		}


		[IdentifiedTest("09c54ba0-04d9-4f6e-9c46-0075612582fa")]
		public void LoadFromLDAP_GoldFlow()
		{
			_testsImplementation.ImportFromLDAPGoldFlow();
		}
	}
}