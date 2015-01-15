using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Provides a means to create Soure Provider in the outside app domain
	/// </summary>
	public interface IProviderFactory
	{
		/// <summary>
		/// Creates the provider based on the identifier.
		/// </summary>
		/// <param name="identifier">A Guid representing the identifier to find the DataSourceProvider.</param>
		/// <returns>A new instance of the datasource provider.</returns>
		IDataSourceProvider CreateProvider(Guid identifier);
	}
}
