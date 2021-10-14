namespace Relativity.IntegrationPoints.Tests.Common.LDAP.TestData
{
	public class AdministrativeTestData : TestDataBase
	{
		public AdministrativeTestData() : base(nameof(AdministrativeTestData), "cn")
		{
		}

		public override string OU => "ou=Administrative";
	}
}
