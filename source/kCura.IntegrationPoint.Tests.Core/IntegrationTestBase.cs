using kCura.Relativity.Client;
namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class IntegrationTestBase
	{
		protected IntegrationTestBase()
		{
			Helper = new Helper();
		}

		public Helper Helper { get; }
	}
}