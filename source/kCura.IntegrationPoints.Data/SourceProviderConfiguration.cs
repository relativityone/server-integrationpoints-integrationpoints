﻿using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data
{
	[Serializable]
	public class SourceProviderConfiguration
	{
		/// <summary>
		/// config to set import native files. 
		/// If this config set to true or false and user set copy native files to true, a new file will be created by copying the orignal native.
		/// If this config set to true and user set copy native files to false, files will be set to linking to the native.
		/// If this config set to false and user set copy native files to false, files will not be imported.
		/// </summary>
		/// <remarks>
		/// only applies when importing doc rdo
		/// </remarks>
		public bool AlwaysImportNativeFiles { get; set; }

		/// <summary>
		/// conguration to set the visibilities of the import settings
		/// </summary>
		public ImportSettingVisibility AvaiableImportSettings { set; get; }

		/// <summary>
		/// Exclusive list of guid associate with Rdo types
		/// </summary>
		public List<Guid> CompatibleRdoTypes { set; get; }
	}
}