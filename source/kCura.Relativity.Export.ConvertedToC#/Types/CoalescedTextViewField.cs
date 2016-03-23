using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using RelativityExportConstants = Relativity.Export.Constants;

namespace kCura.Relativity.Export.Types
{
	public class CoalescedTextViewField : ViewFieldInfo
	{
		public CoalescedTextViewField(ViewFieldInfo vfi, bool useCurrentFieldName) : base(vfi)
		{
			_avfColumnName = RelativityExportConstants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME;
			string nameToUse = RelativityExportConstants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME;
			if (useCurrentFieldName)
				nameToUse = vfi.DisplayName;
			_avfHeaderName = nameToUse;
			_displayName = nameToUse;
		}
	}
}
