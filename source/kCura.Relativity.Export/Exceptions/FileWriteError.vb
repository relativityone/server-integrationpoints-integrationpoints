Namespace kCura.Relativity.Export.Exceptions
	Public Class FileWriteException
		Inherits ExportBaseException
		Public Enum DestinationFile
			Errors
			Load
			Image
			Generic
		End Enum
		Public Sub New(ByVal destination As DestinationFile, ByVal writeError As System.Exception)
			MyBase.new("Error writing to " & destination.ToString & " output file", writeError)
		End Sub

	End Class
End Namespace
