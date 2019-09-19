using System;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Common
{
	internal sealed class DynamicProxyFactoryStub : IDynamicProxyFactory
	{
		public T WrapKeplerService<T>(T keplerService, Func<Task<T>> keplerServiceFactory) where T : class
		{
			return keplerService;
		}
	}
}