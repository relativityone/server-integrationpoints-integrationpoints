namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings
{
	public class FieldClassificationResult : DocumentFieldInfo
	{
		public ClassificationLevel ClassificationLevel { get; set; }
		public string ClassificationReason { get; set; }
	}
}