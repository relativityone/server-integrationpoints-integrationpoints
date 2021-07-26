using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;

namespace Relativity.IntegrationPoints.Tests.Functional.CD
{
	[TestType.UI, TestType.MainFlow]
	public class SyncTests : TestsBase
	{
		private readonly SyncTestsImplementation _testsImplementation;

		public SyncTests()
			: base(nameof(SyncTests))
		{
			_testsImplementation = new SyncTestsImplementation(this);
		}

		public override void OneTimeSetUp()
		{
			base.OneTimeSetUp();
			_testsImplementation.OnSetUpFixture();
		}

		public override void OneTimeTearDown()
		{
			base.OneTimeTearDown();

			_testsImplementation.OnTearDownFixture();
		}

		[IdentifiedTest("11875215-9048-4fa3-91d8-b1cae9b11eef")]
		[TestExecutionCategory.RAPCD.Verification.Functional]
		public void SavedSearch_NativesAndMetadata_GoldFlow()
		{
			_testsImplementation.SavedSearchNativesAndMetadataGoldFlow();
		}
	}
}
