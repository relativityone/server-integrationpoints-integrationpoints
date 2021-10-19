namespace Relativity.IntegrationPoints.Tests.Common.LDAP.TestData
{
	public class ProductDevelopmentTestData : TestDataBase
	{
		public ProductDevelopmentTestData() : base(nameof(ProductDevelopmentTestData), "cn")
		{
		}

		public override string OU => "ou=Product Development";
	}
}
