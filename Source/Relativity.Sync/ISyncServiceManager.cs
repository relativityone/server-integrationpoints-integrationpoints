using System;
using Relativity.API;

namespace Relativity.Sync
{
	/// <summary>
	/// Interface for getting Kepler service
	/// </summary>
	public interface ISyncServiceManager
	{
		/// <summary>
		/// Creates a Kepler client proxy for given interface
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ident"></param>
		/// <returns></returns>
		T CreateProxy<T>(ExecutionIdentity ident) where T : class, IDisposable;


		/// <summary>
		/// Returns Relativity REST service URL
		/// </summary>
		/// <returns></returns>
		Uri GetRESTServiceUrl();
	}
}
