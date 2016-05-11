namespace kCura.IntegrationPoint.Tests.Core
{
	public class HelperBase
	{
		protected static Helper Helper { get; set; }

		public HelperBase(Helper helper)
		{
			Helper = helper;
		}
	}
}
