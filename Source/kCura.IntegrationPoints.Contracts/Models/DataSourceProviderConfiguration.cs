using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Contracts.Models
{
	/// <summary>
	/// Object holding source provider configuration and secured configuration
	/// </summary>
	public class DataSourceProviderConfiguration
	{
		/// <summary>
		/// Initializes new instance of DataSourceProviderConfiguration
		/// </summary>
		public DataSourceProviderConfiguration()
		{
		}

		/// <summary>
		/// Initializes new instance of DataSourceProviderConfiguration
		/// </summary>
		/// <param name="configuration">Source provider configuration</param>
		public DataSourceProviderConfiguration(string configuration)
		{
			Configuration = configuration;
		}

		/// <summary>
		/// Initializes new instance of DataSourceProviderConfiguration
		/// </summary>
		/// <param name="configuration">Source provider configuration</param>
		/// <param name="securedConfiguration">Secured configuration</param>
		public DataSourceProviderConfiguration(string configuration, string securedConfiguration)
		{
			Configuration = configuration;
			SecuredConfiguration = securedConfiguration;
		}

		/// <summary>
		/// Options on a source provider that a user has set.
		/// </summary>
		public string Configuration { get; set; }

		/// <summary>
		/// Secured options that a user has set, in case source provider supports it.
		/// </summary>
		public string SecuredConfiguration { get; set; }
	}
}
