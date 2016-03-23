Imports kCura.Relativity.Export.Types

Namespace kCura.Relativity.Export
	Public Class Utility
		Public Shared Function BuildProxyCharacterDatatable() As DataTable
			Dim i As Int32
			Dim row As ArrayList
			Dim dt As DataTable
			dt = New DataTable
			dt.Columns.Add("Display", GetType(String))
			dt.Columns.Add("CharValue", GetType(Int32))
			Dim rowValue As Char
			Dim rowDisplay As String
			For i = 1 To 255
				row = New ArrayList
				rowDisplay = String.Format("{0} (ASCII:{1})", ChrW(i), i.ToString.PadLeft(3, "0"c))
				row.Add(rowDisplay)
				rowValue = ChrW(i)
				row.Add(rowValue)
				dt.Rows.Add(row.ToArray)
			Next
			Return dt
		End Function

		Public Shared Function GetFieldNamesFromFieldArray(ByVal documentFields As DocumentField()) As String()
			Dim i As Int32
			Dim retval(documentFields.Length - 1) As String
			For i = 0 To retval.Length - 1
				retval(i) = documentFields(i).FieldName
			Next
			Return retval
		End Function

		Public Shared Function GetFilesystemSafeName(ByVal input As String) As String
			Dim output As String = String.Copy(input)
			output = output.Replace("/", " ")
			output = output.Replace(":", " ")
			output = output.Replace("?", " ")
			output = output.Replace("*", " ")
			output = output.Replace("<", " ")
			output = output.Replace(">", " ")
			output = output.Replace("|", " ")
			output = output.Replace("\", " ")
			output = output.Replace("""", " ")
			Return output
		End Function

		''' <summary>
		''' Attempts to determine the encoding for a file by detecting the Byte Order Mark (BOM).
		''' </summary>
		''' <param name="filename">The filename.</param>
		''' <param name="returnEncodingOnly">if set to <c>true</c> [return encoding only].</param>
		''' <param name="performFileExistsCheck">if set to <c>true</c> [perform file exists check].</param>
		''' <returns>
		''' Returns System.Text.Encoding.UTF8, Unicode, or BigEndianUnicode if the BOM is found and Nothing otherwise.
		''' </returns>
		Public Shared Function DetectEncoding(ByVal filename As String, ByVal returnEncodingOnly As Boolean, ByVal performFileExistsCheck As Boolean) As DeterminedEncodingStream
			Dim enc As System.Text.Encoding = Nothing
			Dim filein As System.IO.FileStream = Nothing
			If Not performFileExistsCheck OrElse (performFileExistsCheck AndAlso System.IO.File.Exists(filename)) Then
				filein = New System.IO.FileStream(filename, IO.FileMode.Open, IO.FileAccess.Read)
				If (filein.CanSeek) Then
					Dim bom(4) As Byte
					filein.Read(bom, 0, 4)
					'EF BB BF       = Unicode (UTF-8)
					'FF FE          = ucs-2le, ucs-4le, and ucs-16le OR Unicode
					'FE FF          = utf-16 and ucs-2 OR Unicode (Big-Endian)
					'00 00 FE FF    = ucs-4 OR Unicode (UTF-32 Big-Endian)  NOT SUPPORTING THIS
					'FF FE 00 00		= Unicode (UTF-32) NOT SUPPORTING THIS
					If (((bom(0) = &HEF) And (bom(1) = &HBB) And (bom(2) = &HBF))) Then
						enc = System.Text.Encoding.UTF8
					End If
					If ((bom(0) = &HFF) And (bom(1) = &HFE)) Then
						enc = System.Text.Encoding.Unicode
					End If
					If ((bom(0) = &HFE) And (bom(1) = &HFF)) Then
						enc = System.Text.Encoding.BigEndianUnicode
					End If
					If (bom(0) = &H0 And bom(1) = &H0 And bom(2) = &HFE And bom(3) = &HFF) Then
						'enc = System.Text.Encoding.GetEncoding(12001)	' Unicode (UTF-32 Big-Endian)
					End If
					If (bom(0) = &HFF And bom(1) = &HFE And bom(2) = &H0 And bom(3) = &H0) Then
						'enc = System.Text.Encoding.GetEncoding(12000)	'Unicode (UTF-32)
					End If

					'Position the file cursor back to the start of the file
					filein.Seek(0, System.IO.SeekOrigin.Begin)
				End If
				If returnEncodingOnly Then
					filein.Close()
				End If
			End If
			If returnEncodingOnly Then
				Return New DeterminedEncodingStream(enc)
			Else
				Return New DeterminedEncodingStream(filein, enc)
			End If
		End Function

		''' <summary>
		''' Attempts to determine the encoding for a file by detecting the Byte Order Mark (BOM).
		''' </summary>
		''' <param name="filename">The filename.</param>
		''' <param name="returnEncodingOnly">if set to <c>true</c> [return encoding only].</param>
		''' <returns>
		''' Returns System.Text.Encoding.UTF8, Unicode, or BigEndianUnicode if the BOM is found and Nothing otherwise.
		''' </returns>
		Public Shared Function DetectEncoding(ByVal filename As String, ByVal returnEncodingOnly As Boolean) As DeterminedEncodingStream
			Return DetectEncoding(filename, returnEncodingOnly, True)
		End Function
	End Class

	Public Class DeterminedEncodingStream
		Private _fileStream As System.IO.FileStream
		Private _determinedEncoding As System.Text.Encoding

		Public ReadOnly Property UnderlyingStream() As System.IO.Stream
			Get
				Return _fileStream
			End Get
		End Property

		Public ReadOnly Property DeterminedEncoding() As System.Text.Encoding
			Get
				Return _determinedEncoding
			End Get
		End Property

		Public Sub New(ByVal fileStream As System.IO.FileStream, ByVal determinedEncoding As System.Text.Encoding)
			_fileStream = fileStream
			_determinedEncoding = determinedEncoding
		End Sub

		Public Sub New(ByVal determinedEncoding As System.Text.Encoding)
			_determinedEncoding = determinedEncoding
		End Sub

		Public Sub Close()
			Try
				If Not _fileStream Is Nothing Then _fileStream.Close()
			Catch
			End Try
		End Sub

	End Class
End Namespace