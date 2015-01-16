using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		/// value that should be used when mapping
		/// </summary>
		public string FieldIdentifier { get; set; }

		public FieldType FieldType { get; set; }

		
		public bool IsIdentifier { get; set; }

	}
}
