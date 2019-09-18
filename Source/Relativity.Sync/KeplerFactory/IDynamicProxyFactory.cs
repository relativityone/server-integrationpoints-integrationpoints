using System;

namespace Relativity.Sync.KeplerFactory
{
	internal interface IDynamicProxyFactory
	{
		T WrapKeplerService<T>(T keplerService, Func<T> keplerServiceFactory) where T : class;
	}
}