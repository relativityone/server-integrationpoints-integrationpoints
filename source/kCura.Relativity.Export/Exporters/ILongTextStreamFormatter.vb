
Imports kCura.Relativity.Export.FileObjects

Namespace kCura.Relativity.Export.Exports
	Public Interface ILongTextStreamFormatter
		Sub TransformAndWriteCharacter(ByVal character As Int32, ByVal outputStream As System.IO.TextWriter)
	End Interface
	Public Class NonTransformFormatter
		Implements ILongTextStreamFormatter
		Public Sub TransformAndWriteCharacter(ByVal character As Integer, ByVal outputStream As System.IO.TextWriter) Implements ILongTextStreamFormatter.TransformAndWriteCharacter
			outputStream.Write(ChrW(character))
		End Sub
	End Class
	Public Class HtmlFileLongTextStreamFormatter
		Implements ILongTextStreamFormatter

		Private _source As System.IO.TextReader

		Public Sub New(ByVal settings As ExportFile, ByVal source As System.IO.TextReader)
			_source = source
		End Sub

		Public Sub TransformAndWriteCharacter(ByVal character As Integer, ByVal outputStream As System.IO.TextWriter) Implements ILongTextStreamFormatter.TransformAndWriteCharacter
			Select Case character
				Case 13
					outputStream.Write("<br/>")
					If _source.Peek = 10 Then _source.Read()
				Case 10
					outputStream.Write("<br/>")
				Case Else
					outputStream.Write(System.Web.HttpUtility.HtmlEncode(ChrW(character)))
			End Select
		End Sub

	End Class

	Public Class DelimitedFileLongTextStreamFormatter
		Implements ILongTextStreamFormatter
		Private _quoteDelimiter As Char
		Private _newlineDelimiter As Char
		Private _source As System.IO.TextReader

		Public Sub New(ByVal settings As ExportFile, ByVal source As System.IO.TextReader)
			_quoteDelimiter = settings.QuoteDelimiter
			_newlineDelimiter = settings.NewlineDelimiter
			_source = source
		End Sub

		Public Sub TransformAndWriteCharacter(ByVal character As Int32, ByVal outputStream As System.IO.TextWriter) Implements ILongTextStreamFormatter.TransformAndWriteCharacter
			Select Case character
				Case AscW(_quoteDelimiter)
					outputStream.Write(_quoteDelimiter & _quoteDelimiter)
				Case 13, 10
					outputStream.Write(_newlineDelimiter)
					If _source.Peek = 10 Then
						_source.Read()
					End If
				Case Else
					outputStream.Write(ChrW(character))
			End Select
		End Sub

	End Class


End Namespace

