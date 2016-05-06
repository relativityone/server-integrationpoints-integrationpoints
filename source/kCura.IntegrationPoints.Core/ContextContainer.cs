using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class ContextContainer : IContextContainer
	{
		public IHelper Helper { get; }

		public ContextContainer(IHelper helper)
		{
			Helper = helper;
		}
	}
}
