namespace Relativity.IntegrationPoints.Tests.Common.LDAP.TestData
{
	public class HumanResourcesTestData : TestDataBase
	{
		public HumanResourcesTestData() : base(nameof(HumanResourcesTestData), "cn")
		{
		}

		public override string OU => "ou=Human Resources";
	}
}
