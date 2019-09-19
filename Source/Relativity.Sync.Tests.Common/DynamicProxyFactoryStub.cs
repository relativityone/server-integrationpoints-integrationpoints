using System.Diagnostics.CodeAnalysis;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Common
{
	[ExcludeFromCodeCoverage]
	internal sealed class DynamicProxyFactoryStub : IDynamicProxyFactory
	{
		public T WrapKeplerService<T>(T keplerService)
		{
			return keplerService;
		}
	}
}