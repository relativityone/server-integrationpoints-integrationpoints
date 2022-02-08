using System;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class FieldClassificationResult
	{
		public FieldInfo FieldInfo { get; }

		public ClassificationLevel ClassificationLevel { get; set; }
		public string ClassificationReason { get; set; }

		public FieldClassificationResult(FieldInfo fieldInfo)
		{
			FieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
		}
	}
}