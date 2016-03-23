Namespace kCura.Relativity.Export.Exceptions
	Public MustInherit Class ExportBaseException
		Inherits System.Exception
		Protected Sub New(ByVal message As String, ByVal innerException As System.Exception)
			MyBase.new(message, innerException)
		End Sub
	End Class
End Namespace