using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class ContextContainer : IContextContainer
	{
		public IHelper Helper { get; }

		public IServicesMgr ServicesMgr { get; }

		internal ContextContainer(IHelper helper)
		{
			Helper = helper;
			ServicesMgr = helper.GetServicesManager();
		}

		internal ContextContainer(IHelper helper, IServicesMgr servicesMgr)
		{
			Helper = helper;
			ServicesMgr = servicesMgr;
		}
	}
}
