using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.System.Stubs
{
	internal sealed class DynamicProxyFactoryStub : IDynamicProxyFactory
	{
		public T WrapKeplerService<T>(T keplerService)
		{
			return keplerService;
		}
	}
}