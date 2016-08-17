namespace kCura.IntegrationPoint.Tests.Core.Models
{
	public class ImportOverwriteMode
	{
		private ImportOverwriteMode(string value)
		{
			Value = value;
		}

		public string Value { get; set; }

		public static ImportOverwriteMode Append => new ImportOverwriteMode("Append");
		public static ImportOverwriteMode AppendOverlay => new ImportOverwriteMode("AppendOverlay");
		public static ImportOverwriteMode Overlay => new ImportOverwriteMode("Overlay");
	}
}