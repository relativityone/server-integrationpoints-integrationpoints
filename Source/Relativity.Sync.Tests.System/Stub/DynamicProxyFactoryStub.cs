﻿using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.System.Stub
{
	internal sealed class DynamicProxyFactoryStub : IDynamicProxyFactory
	{
		public T WrapKeplerService<T>(T keplerService)
		{
			return keplerService;
		}
	}
}