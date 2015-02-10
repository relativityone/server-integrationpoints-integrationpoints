using System;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	/// <summary>
	/// A C# object that describes a source provider registration.
	/// </summary>
	public class SourceProvider
	{
		/// <summary>
		/// The source provider guid used to identify the provider
		/// </summary>
		internal Guid GUID { get; set; }
		
		internal Guid ApplicationGUID { get; set; }
		internal int ApplicationID { get; set; }
		/// <summary>
		/// The display name of the source provider, used to show a user which provider will be used
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// The url fot the configuration that will be used in the setting of the providers settings.
		/// </summary>
		public string Url { get; set; }
	}
}
