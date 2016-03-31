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
			DestinationFieldName = Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME;
		}
	}
}
