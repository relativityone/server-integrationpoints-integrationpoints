Namespace kCura.Relativity.Export
	Public Class Settings
		''' -----------------------------------------------------------------------------
		''' <summary>
		'''		Default timeout wait time for Web Service in Milliseconds.
		'''		Set to 1 minute (2005-08-31).
		''' </summary>
		''' <remarks>
		''' </remarks>
		''' <history>
		''' 	[nkapuza]	8/31/2005	Created
		''' </history>
		''' -----------------------------------------------------------------------------
		Public Shared DefaultTimeOut As Int32 = Config.WebAPIOperationTimeout
		Public Shared AuthenticationToken As String = String.Empty
		Public Const MAX_STRING_FIELD_LENGTH As Int32 = 1048576
		Public Shared SendEmailOnLoadCompletion As Boolean = False

	End Class
End Namespace