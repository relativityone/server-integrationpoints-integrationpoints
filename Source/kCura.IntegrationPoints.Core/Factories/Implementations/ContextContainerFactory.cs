using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ContextContainerFactory : IContextContainerFactory
	{
		public IContextContainer CreateContextContainer(IHelper helper)
		{
			return new ContextContainer(helper);
		}

		public IContextContainer CreateContextContainer(IHelper helper, IServicesMgr servicesMgr)
		{
			return new ContextContainer(helper, servicesMgr);
		}
	}
}