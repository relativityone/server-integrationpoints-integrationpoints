using System;

namespace kCura.IntegrationPoints.Contracts.Models
{
	public enum FieldType
	{
		String = 1
	}

	/// <summary>
	/// Responsible for going to a datasource and returning what fields can be mapped
	/// </summary>
	[Serializable]
	public class FieldEntry
	{
		/// <summary>
		/// Friendly name that will be displayed
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Field identifier. This is the value that should be used when mapping.
		/// </summary>
		public string FieldIdentifier { get; set; }

		/// <summary>
		/// Field type. 
		/// </summary>
		public FieldType FieldType { get; set; }

		/// <summary>
		/// This flag indicates if field will contain data unique identifier 
		/// </summary>
		public bool IsIdentifier { get; set; }

		/// <summary>
		/// Determines if the field is required to be mapped
		/// </summary>
		public bool IsRequired { get; set; }
	}
}
