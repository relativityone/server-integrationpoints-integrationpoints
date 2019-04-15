using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Common
{
	internal sealed class DynamicProxyFactoryStub : IDynamicProxyFactory
	{
		public T WrapKeplerService<T>(T keplerService)
		{
			return keplerService;
		}
	}
}