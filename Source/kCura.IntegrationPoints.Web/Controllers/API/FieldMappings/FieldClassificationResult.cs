namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings
{
	public class FieldClassificationResult
	{
		public string Name { get; set; }
		public string FieldIdentifier { get; set; }
		public string Type { get; set; }
		public bool IsIdentifier { get; set; }
		public ClassificationLevel ClassificationLevel { get; set; }
		public string ClassificationReason { get; set; }
	}
}