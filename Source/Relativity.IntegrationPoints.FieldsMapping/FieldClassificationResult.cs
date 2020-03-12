namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class FieldClassificationResult
	{
		public DocumentFieldInfo FieldInfo { get; }

		public ClassificationLevel ClassificationLevel { get; set; }
		public string ClassificationReason { get; set; }

		public FieldClassificationResult(DocumentFieldInfo fieldInfo)
		{
			FieldInfo = fieldInfo;
		}
	}
}