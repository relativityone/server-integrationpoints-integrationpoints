namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class NativeFileImportService
	{
		public bool ImportNativeFiles { get; set; }
		public string SourceFieldName { get; set; }
		public string DestinationFieldName { get; set; }

		public NativeFileImportService()
		{
			ImportNativeFiles = false;
			DestinationFieldName = "NATIVE_FILE_PATH_001";
		}
	}
}
