using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class ContextContainer : IContextContainer
	{
		public IHelper Helper { get; }

		internal ContextContainer(IHelper helper)
		{
			Helper = helper;
		}
	}
}
