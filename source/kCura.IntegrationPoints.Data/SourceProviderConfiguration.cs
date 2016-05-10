using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data
{
	[Serializable]
	public class SourceProviderConfiguration
	{
		/// <summary>
		/// Config to set import native files. 
		/// If this config set to true or false and user set copy native files to true, a new file will be created by copying the orignal native.
		/// If this config set to true and user set copy native files to false, files will be set to linking to the native.
		/// If this config set to false and user set copy native files to false, files will not be imported.
		/// </summary>
		/// <remarks>
		/// only applies when importing doc rdo
		/// </remarks>
		public bool AlwaysImportNativeFiles { get; set; }

		/// <summary>
		/// configuration to set the visibilities of the import settings
		/// </summary>
		/// <remarks>
		/// Only available for internal providers
		/// </remarks>
		internal ImportSettingVisibility AvailableImportSettings { set; get; }

		/// <summary>
		/// Exclusive list of guid associate with Rdo types
		/// </summary>
		public List<Guid> CompatibleRdoTypes { set; get; }

		public bool GetDataProvideAllFieldsRequired { set; get; }

		/// <summary>
		/// Configuration to import the native's file name.
		/// If true, we will pass to the import API the native's file name.
		/// If false, we will not pass to the import API the native's file name.
		/// </summary>
		/// <remarks>This configuration value only applies to the Document RDO.</remarks>
		public bool AlwaysImportNativeFileNames { get; set; }

		/// <summary>
		/// A setting to prevent user to map identifier field to a non identifier field.
		/// </summary>
		public bool OnlyMapIdentifierToIdentifier { get; set; }
	}
}