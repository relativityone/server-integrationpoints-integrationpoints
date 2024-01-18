using System;
using System.Runtime.Serialization;

namespace Relativity.IntegrationPoints.Contracts.Internals
{
	/// <summary>
	/// This type is only for internal use in Integration Points.
	/// </summary>
	///
	/// This class is public because we do not want to add 'Internals Visible To' to many projects
	/// since it would force us to generate new sdk every time we would want to use it in new project.
	/// https://einstein.kcura.com/display/DV/Internal+classes+in+RIP+SDK
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
