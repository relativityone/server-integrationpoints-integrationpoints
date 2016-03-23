Imports kCura.Relativity.Export.Exports
Imports kCura.Relativity.Export.FileObjects

Namespace kCura.Relativity.Export.Exports
	Public Class DelimitedCellFormatter
		Implements ILoadFileCellFormatter

		Private _settings As ExportFile
		Public Sub New(ByVal settings As ExportFile)
			_settings = settings
		End Sub
		Public Function TransformToCell(ByVal contents As String) As String Implements ILoadFileCellFormatter.TransformToCell
			contents = contents.Replace(System.Environment.NewLine, ChrW(10).ToString)
			contents = contents.Replace(ChrW(13), ChrW(10))
			contents = contents.Replace(ChrW(10), _settings.NewlineDelimiter)
			contents = contents.Replace(_settings.QuoteDelimiter, _settings.QuoteDelimiter & _settings.QuoteDelimiter)
			Return String.Format("{0}{1}{0}", _settings.QuoteDelimiter, contents)
		End Function

		Public ReadOnly Property RowPrefix() As String Implements ILoadFileCellFormatter.RowPrefix
			Get
				Return String.Empty
			End Get
		End Property

		Public ReadOnly Property RowSuffix() As String Implements ILoadFileCellFormatter.RowSuffix
			Get
				Return String.Empty
			End Get
		End Property

		Public Function CreateImageCell(ByVal artifact As ObjectExportInfo) As String Implements ILoadFileCellFormatter.CreateImageCell
			Return String.Empty
		End Function

		Public Function CreateNativeCell(ByVal location As String, ByVal artifact As ObjectExportInfo) As String Implements ILoadFileCellFormatter.CreateNativeCell
			Return String.Format("{2}{0}{1}{0}", _settings.QuoteDelimiter, location, _settings.RecordDelimiter)
		End Function
	End Class
End Namespace
