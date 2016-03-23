Imports kCura.Relativity.Export.Exports

Namespace kCura.Relativity.Export.Exports
	Public Interface ILoadFileCellFormatter
		Function TransformToCell(ByVal contents As String) As String
		Function CreateNativeCell(ByVal location As String, ByVal artifact As ObjectExportInfo) As String
		Function CreateImageCell(ByVal artifact As ObjectExportInfo) As String

		ReadOnly Property RowPrefix() As String
		ReadOnly Property RowSuffix() As String
	End Interface





	
End Namespace

