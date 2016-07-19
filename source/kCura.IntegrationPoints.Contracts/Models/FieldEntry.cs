﻿using System;

namespace kCura.IntegrationPoints.Contracts.Models
{
	
    /// <summary>
    /// Specifies the data type of a field.
    /// </summary>
    public enum FieldType
	{
		/// <summary>
		/// The field type of String.
		/// </summary>
        String = 1
	}

	/// <summary>
	/// Retrieves fields from the data source that users can map in the Relativity UI.
	/// </summary>
	[Serializable]
	public class FieldEntry
	{
		private string _actualName;

		/// <summary>
		/// Gets or set a user-friendly name for display in the Relativity UI.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets a field identifier used when mapping data source fields to workspace fields.
		/// </summary>
		public string FieldIdentifier { get; set; }

		/// <summary>
		/// Represents the name used for the field in the source code.
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
		/// Gets or sets the field type. 
		/// </summary>
		public FieldType FieldType { get; set; }

		/// <summary>
		/// Gets or sets a flag indicating whether the field contains a unique identifier for the data.
		/// </summary>
		public bool IsIdentifier { get; set; }

		/// <summary>
		/// Gets or sets a flag indicating whether a field in the data source must be mapped.
		/// </summary>
		public bool IsRequired { get; set; }
	}
}
