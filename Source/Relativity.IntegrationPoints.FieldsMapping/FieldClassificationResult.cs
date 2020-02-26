namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class FieldClassificationResult
	{
		private readonly DocumentFieldInfo _fieldInfo;

		public ClassificationLevel ClassificationLevel { get; set; }
		public string ClassificationReason { get; set; }

		public string Name => _fieldInfo.Name;
		public string FieldIdentifier => _fieldInfo.FieldIdentifier;
		public string Type => _fieldInfo.DisplayType;
		public int Length => _fieldInfo.Length;
		public bool IsIdentifier => _fieldInfo.IsIdentifier;
		public bool IsRequired => _fieldInfo.IsRequired;

		public DocumentFieldInfo GetFieldInfo() => _fieldInfo;

		public FieldClassificationResult(DocumentFieldInfo fieldInfo)
		{
			_fieldInfo = fieldInfo;
		}
	}
}