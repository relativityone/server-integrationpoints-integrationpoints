using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Contracts.Internals;

namespace Relativity.IntegrationPoints.Contracts
{
	/// <summary>
	/// Represents configurations for a source provider.
	/// </summary>
	[Serializable]
	public class SourceProviderConfiguration
	{
		/// <summary>
		/// Initializes new instance of SourceProviderConfiguration.
		/// </summary>
		public SourceProviderConfiguration()
		{
		}

		/// <summary>
		/// Gets or sets a Boolean value used in a configuration to determine whether native files should be linked to or imported into Relativity.
		/// </summary>
		/// <remarks>
		/// This configuration value only applies to RDOs with a parent type of Document as follows:
		/// <list type="bullet">
		/// <item><description>If this property is set to true or false, and the user sets 'Copy Native Files' to true, Relativity creates new files by copying the original files.</description></item>
		/// <item><description>If this property is set to true, and the user sets 'Copy Native Files' to false, Relativity creates new files by linking to the original files.</description></item>
		/// <item><description>If this property is set to false, and the user sets 'Copy Native Files' to false, Relativity does not import the files.</description></item>
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
		/// Gets or sets a list of GUIDs for RDOs that the provider is compatible with.
		/// </summary>
		public List<Guid> CompatibleRdoTypes { set; get; }

		/// <summary>
		/// Gets or sets a Boolean value used in a configuration to determine whether the name of the native file should be imported.
		/// </summary>
		/// <remarks>
		/// This configuration value only applies to RDOs with a parent type of Document follows:
		/// <list type="bullet">
		/// <item><description>If this property is set to true, the name of the native file is passed to the Import API.</description></item>
		/// <item><description>If this property is set to false, the name of the native file is not passed to the Import API.</description></item>
		/// </list>
		/// </remarks>
		public bool AlwaysImportNativeFileNames { get; set; }

		/// <summary>
		/// Get or sets a Boolean value that prevents a user from mapping an identifier field to a non-identifier field.
		/// </summary>
		public bool OnlyMapIdentifierToIdentifier { get; set; }
	}
}