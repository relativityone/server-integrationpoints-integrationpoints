namespace Relativity.Sync.KeplerFactory
{
	internal interface IDynamicProxyFactory
	{
		T WrapKeplerService<T>(T keplerService);
	}
}