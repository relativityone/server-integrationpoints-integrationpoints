namespace Relativity.Sync.Proxy
{
	internal interface IDynamicProxyFactory
	{
		T WrapKeplerService<T>(T keplerService);
	}
}