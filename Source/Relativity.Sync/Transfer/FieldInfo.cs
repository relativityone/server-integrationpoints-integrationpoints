namespace Relativity.Sync.Transfer
{
	internal sealed class FieldInfo
	{
		/// <summary>
		/// Gets or sets a user-friendly name for display in the Relativity UI.
		/// </summary>
		public string DisplayName { get; set; }

		public SpecialFieldType SpecialFieldType { get; set; } = SpecialFieldType.None;

		public RelativityDataType RelativityDataType { get; set; }

		public bool IsDocumentField { get; set; }

		public int DocumentFieldIndex { get; set; } = -1;
	}
}