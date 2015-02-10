using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// The attribute used to decorate a class to find out which providers exist in this app domian
	/// </summary>
	public class DataSourceProviderAttribute : Attribute
	{
		/// <summary>
		/// Identifier to represent the data source provider
		/// </summary>
		public Guid Identifier { get; private set; }

		/// <summary>
		/// Create a new instance of the Data source provider attribute.
		/// </summary>
		/// <param name="identifier">A guid representing the unique identifier for the data source provider.</param>
		public DataSourceProviderAttribute(string identifier)
		{
			this.Identifier = Guid.Parse(identifier);
		}
	}
}
