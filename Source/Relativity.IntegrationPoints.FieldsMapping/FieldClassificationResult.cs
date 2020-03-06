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

		// to be compatible with old JS code
		public string DisplayName => Name;

		// to be compatible with old JS code
		public string ActualName => Name + (string.IsNullOrEmpty(Type) ? "" : $" [{_fieldInfo.DisplayType}]");

		public DocumentFieldInfo GetFieldInfo() => _fieldInfo;

		public FieldClassificationResult(DocumentFieldInfo fieldInfo)
		{
			_fieldInfo = fieldInfo;
		}
	}
}