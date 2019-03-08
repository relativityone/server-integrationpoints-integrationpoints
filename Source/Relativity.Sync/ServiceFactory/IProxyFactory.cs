using System;
using System.Threading.Tasks;

namespace Relativity.Sync.ServiceFactory
{
	internal interface IProxyFactory
	{
		Task<T> CreateProxyAsync<T>() where T : IDisposable;
	}
}