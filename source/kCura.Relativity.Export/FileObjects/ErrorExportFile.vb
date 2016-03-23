Namespace kCura.Relativity.Export.FileObjects

	Public Class ErrorExportFile
		Inherits ExportFile
		Private _errorMessage As String = String.Empty
		Public Sub New(ByVal errorMessage As String)
			MyBase.New(-1)
			If String.IsNullOrEmpty(errorMessage) Then Throw New System.ArgumentException("Error message cannot be null for an error export file")
			_errorMessage = errorMessage
		End Sub
		Public ReadOnly Property ErrorMessage As String
			Get
				Return _errorMessage
			End Get
		End Property
	End Class
End Namespace
