using System;

namespace Relativity.Sync.ServiceFactory
{
	internal interface IProxyFactory
	{
		T CreateProxy<T>() where T : IDisposable;
	}
}