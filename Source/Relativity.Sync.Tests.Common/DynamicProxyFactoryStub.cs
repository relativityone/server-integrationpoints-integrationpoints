using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Common
{
	[ExcludeFromCodeCoverage]
	internal sealed class DynamicProxyFactoryStub : IDynamicProxyFactory
	{
		public T WrapKeplerService<T>(T keplerService, Func<Task<T>> keplerServiceFactory) where T : class
		{
			return keplerService;
		}
	}
}