using System;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	/// <summary>
	/// Provides the information for the registration of a data source provider.
	/// </summary>
	public class SourceProvider
	{
		/// <summary>
		/// Gets or sets the GUID identifying the data source provider.
		/// </summary>
		internal Guid GUID { get; set; }
		
		internal Guid ApplicationGUID { get; set; }
		internal int ApplicationID { get; set; }
		/// <summary>
		/// Gets or sets the name of the data source provider displayed in the Relativity UI.
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the URL used to configure the data source provider.
		/// </summary>
		public string Url { get; set; }
	}
}
