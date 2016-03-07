using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data
{
	[DataContract]
	public class ImportSettingVisibility
	{
		[NonSerialized]
		private bool _allowUserToMapNativeFileField = true;

		/// <summary>
		/// To enable visibilities of native file path mapping.
		/// </summary>
		/// <remarks>
		/// This option only apply if document object is selected.
		/// </remarks>
		[DataMember]
		public bool AllowUserToMapNativeFileField
		{
			set { _allowUserToMapNativeFileField = value; }
			get { return _allowUserToMapNativeFileField; }
		}
	}
}
