namespace Relativity.IntegrationPoints.Tests.Common.LDAP.TestData
{
	public class ManagementTestData : TestDataBase
	{
		public ManagementTestData() : base(nameof(ManagementTestData), "uid")
		{
		}

		public override string OU => "ou=Management";
	}
}
