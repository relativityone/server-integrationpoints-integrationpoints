using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Data
{
	/// <summary>
	/// This class contains configurations for a source provider.
	/// </summary>
	[Serializable]
	public class SourceProviderConfiguration
	{
		/// <summary>
		/// Configuration to set import native files. 
		/// </summary>
		/// <remarks>
		/// This configuration value only applies to the Document RDO.
		/// <list type="bullet">
		/// <item><description>If this config is set to true or false and the user sets 'copy native files' to true, a new file will be created by copying the orignal native.</description></item>
		/// <item><description>If this config is set to true and the user sets 'copy native files' to false, files will link to the native.</description></item>
		/// <item><description>If this config is set to false and the user sets 'copy native files' to false, files will not be imported.</description></item>
		/// </list>
		/// </remarks>
		public bool AlwaysImportNativeFiles { get; set; }

		/// <summary>
		/// Configuration to set the visibilities of the import settings
		/// </summary>
		/// <remarks>
		/// Only available for internal providers.
		/// </remarks>
		[JsonProperty()]
		internal ImportSettingVisibility AvailableImportSettings { set; get; }

		/// <summary>
		/// List of guids of RDOs that the provider is compatible with.
		/// </summary>
		public List<Guid> CompatibleRdoTypes { set; get; }

		/// <summary>
		/// Configuration to import the native's file name.
		/// </summary>
		/// <remarks>
		/// This configuration value only applies to the Document RDO.
		/// <list type="bullet">
		/// <item><description>If true, we will pass to the import API the native's file name.</description></item>
		/// <item><description>If false, we will not pass to the import API the native's file name.</description></item>
		/// </list>
		/// </remarks>
		public bool AlwaysImportNativeFileNames { get; set; }

		/// <summary>
		/// Configuration to prevent user from mapping the identifier field to a non identifier field.
		/// </summary>
		public bool OnlyMapIdentifierToIdentifier { get; set; }
	}
}