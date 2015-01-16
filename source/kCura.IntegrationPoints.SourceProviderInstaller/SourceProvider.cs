using System;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public class SourceProvider
	{
		internal Guid GUID { get; set; }
		internal Guid ApplicationGUID { get; set; }
		internal int ApplicationID { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
	}
}
