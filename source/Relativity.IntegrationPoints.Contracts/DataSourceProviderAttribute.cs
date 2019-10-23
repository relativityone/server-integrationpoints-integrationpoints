using System;

namespace Relativity.IntegrationPoints.Contracts
{
	/// <summary>
	/// Used to determine the providers that exist in a specific application domain.
	/// </summary>
	public class DataSourceProviderAttribute : Attribute
	{
		/// <summary>
		/// Gets the identifier for the data source provider.
		/// </summary>
		public Guid Identifier { get; private set; }

		/// <summary>
        /// Creates a new instance of the DataSourceProviderAttribute class.
		/// </summary>
        /// <param name="identifier">A GUID used to identify the data source provider.</param>
		public DataSourceProviderAttribute(string identifier)
		{
			this.Identifier = Guid.Parse(identifier);
		}
	}
}
