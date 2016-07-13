using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.Contracts
{
	[DataContract]
	internal class ImportSettingVisibility
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
