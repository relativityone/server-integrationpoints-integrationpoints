Imports RelativityExportConstants = Relativity.Export.Constants

Namespace kCura.Relativity.Export.Types
	Public Class CoalescedTextViewField
		Inherits ViewFieldInfo
		Public Sub New(ByVal vfi As ViewFieldInfo, ByVal useCurrentFieldName As Boolean)
			MyBase.New(vfi)
			_avfColumnName = RelativityExportConstants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME
			Dim nameToUse As String = RelativityExportConstants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME
			If useCurrentFieldName Then nameToUse = vfi.DisplayName
			_avfHeaderName = nameToUse
			_displayName = nameToUse
		End Sub
	End Class
End Namespace