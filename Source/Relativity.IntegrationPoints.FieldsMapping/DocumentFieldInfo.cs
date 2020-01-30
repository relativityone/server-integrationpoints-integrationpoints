namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class DocumentFieldInfo
	{
		public string Name { get; set; }
		public string FieldIdentifier { get; set; }
		public string Type { get; set; }
		public int Length { get; set; }

		public bool IsIdentifier { get; set; }
		public bool IsRequired { get; set; }

	}
}