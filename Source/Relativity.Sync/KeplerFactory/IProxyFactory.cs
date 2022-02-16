using System;
using System.Threading.Tasks;

namespace Relativity.Sync.KeplerFactory
{
    /// <summary>
    /// Interface for getting Kepler service
    /// </summary>
	public interface IProxyFactory
	{
        /// <summary>
        /// Creates a Kepler client proxy for given interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
		Task<T> CreateProxyAsync<T>() where T : class, IDisposable;
	}
}