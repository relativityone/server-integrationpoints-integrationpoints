using System;

namespace Relativity.Sync.Executors
{
	/// <summary>
	/// Retrieves fields from the data source that users can map in the Relativity UI.
	/// </summary>
	[Serializable]
	public class FieldEntry
	{
		private string _actualName;

		/// <summary>
		/// Gets or sets a user-friendly name for display in the Relativity UI.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets a field identifier used when mapping data source fields to workspace fields.
		/// </summary>
		public string FieldIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the name of the field used in the source code.
		/// </summary>
		/// <remarks>The value for this property is frequently the display name for a field without spaces.</remarks>
		public string ActualName
		{
			get
			{
				if (_actualName == null)
				{
					_actualName = IsIdentifier ? DisplayName.Replace(" [Object Identifier]", String.Empty) : DisplayName;
				}
				return _actualName;
			}
		}

		/// <summary>
		/// Gets or sets a flag indicating whether the field contains a unique identifier for the data.
		/// </summary>
		public bool IsIdentifier { get; set; }

	}
}