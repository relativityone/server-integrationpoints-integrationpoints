using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Includes optional properties that control source provider behavior.
	/// </summary>
	[Serializable]
	public class SourceProviderConfiguration
	{
		/// <summary>
		/// Indicates whether the Integration Point framework is set to import native files.
		/// </summary>
		/// <remarks>
		/// This configuration property applies only to Document objects acting as destination RDOs. It works in conjunction with the Copy Native File setting that users can select when mapping fields for an integration point through the Relativity UI. 
		/// <list type="bullet">
		/// <item><description>IIf this configuration property is either true or false, and the user selects Copy Native File, then Relativity creates a new file by copying the original native file.</description></item>
		/// <item><description>If this configuration property is true, and the user doesn’t select Copy Native File, then Relativity creates a link to the native file.</description></item>
		/// <item><description>If this configuration property is false, and the user doesn’t select Copy Native File, then Relativity won’t import any native files.</description></item>
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
		/// Provides a list of GUIDs for RDOs that are compatible with the provider.
		/// </summary>
		public List<Guid> CompatibleRdoTypes { set; get; }

		/// <summary>
		/// Indicates whether the Integration Point framework is set to import the names of native files.
		/// </summary>
		/// <remarks>
		/// This configuration property applies only to Document objects acting as destination RDOs. The Integration Points framework passes the name of a native file to the Import API only when this property is set to true.
		/// <list type="bullet">
		/// <item><description>If true, we will pass to the import API the native's file name.</description></item>
		/// <item><description>If false, we will not pass to the import API the native's file name.</description></item>
		/// </list>
		/// </remarks>
		public bool AlwaysImportNativeFileNames { get; set; }

		/// <summary>
		/// Prevents the user from mapping an identifier field to a non-identifier field.
		/// </summary>
		/// <remarks>This property applies only when a user attempts to map two RDOs, which contain a field designated as an identifier field, such as Control Number on the Document RDO.
		///</remarks>
		public bool OnlyMapIdentifierToIdentifier { get; set; }
	}
}