using System;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
    /// Provides a method for creating a data source provider in an external application domain.
	/// </summary>
	public interface IProviderFactory
	{
		/// <summary>
		/// Creates a provider using the GUID specified as an identifier.
		/// </summary>
        /// <param name="identifier">A Guid used to identify the data source provider.</param>
        /// <returns>A new instance of a data source provider.</returns>
		IDataSourceProvider CreateProvider(Guid identifier);
	}
}
