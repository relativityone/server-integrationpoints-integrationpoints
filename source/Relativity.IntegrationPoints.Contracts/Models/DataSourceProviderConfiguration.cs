using System;

namespace Relativity.IntegrationPoints.Contracts.Models
{
	/// <summary>
	/// Represents general and secured source provider configurations set by a user.
	/// </summary>
	[Serializable]
	public class DataSourceProviderConfiguration
	{
		/// <summary>
		/// Initializes an instance of the DataSourceProviderConfiguration class.
		/// </summary>
		public DataSourceProviderConfiguration()
		{
		}

		/// <summary>
		/// Initializes an instance of the DataSourceProviderConfiguration class with a string for a general configuration.
		/// </summary>
		/// <param name="configuration">A group of configuration settings that the control source provider behavior.</param>
		public DataSourceProviderConfiguration(string configuration)
		{
			Configuration = configuration;
		}

		/// <summary>
		/// Initializes an instance of the DataSourceProviderConfiguration class with a string for a general configuration and a string for a secured configuration.
		/// </summary>
		/// <param name="configuration">A group of configuration settings that the control source provider behavior.</param>
		/// <param name="securedConfiguration">A group of secured configuration settings that the control source provider behavior.</param>
		public DataSourceProviderConfiguration(string configuration, string securedConfiguration)
		{
			Configuration = configuration;
			SecuredConfiguration = securedConfiguration;
		}

		/// <summary>
		/// Gets or sets options that the user set on a source provider. 
		/// </summary>
		public string Configuration { get; set; }

		/// <summary>
		/// Gets or sets any supported secured options that the user set on a source provider. 
		/// </summary>
		public string SecuredConfiguration { get; set; }
	}
}
