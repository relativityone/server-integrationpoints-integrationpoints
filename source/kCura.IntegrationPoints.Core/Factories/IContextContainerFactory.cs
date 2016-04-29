using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IContextContainerFactory
	{
		IContextContainer CreateContextContainer(IHelper helper);
	}
}
