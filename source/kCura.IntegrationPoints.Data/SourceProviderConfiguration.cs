using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.Data
{
	[Serializable]
	public class SourceProviderConfiguration
	{
		/// <summary>
		/// Exclusive list of guid associate with Rdo types
		/// </summary>
		public List<Guid> CompatibleRdoTypes { set; get; }

		public ImportSettingVisibility AvaiableImportSettings { set; get; }
	}
}