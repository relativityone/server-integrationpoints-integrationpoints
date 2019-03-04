using System;

namespace Relativity.Sync.ServiceFactory
{
	public interface IProxyFactory
	{
		T CreateProxy<T>() where T : IDisposable;
	}
}