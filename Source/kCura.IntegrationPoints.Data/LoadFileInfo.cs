using System;

namespace kCura.IntegrationPoints.Data
{
	public class LoadFileInfo
	{
		public long Size { get; set; }

		public DateTime LastModifiedDate { get; set; }

		public string FullPath { get; set; }
	}
}
