Namespace PropertyExtractor
	Public Class EML

		Public Function Extract(ByVal fileName As String) As System.Collections.Hashtable

			Dim sr As New System.IO.StreamReader(fileName)
			Dim lineBuffer As String
			Dim header As String = Nothing
			Dim headerValue As String = Nothing

			Dim body As New System.Text.StringBuilder
			Dim inBody As Boolean

			Dim hashTable As New System.Collections.Hashtable
			hashTable.Add("File Type".ToLower, "Email Message")

			lineBuffer = sr.ReadLine

			Do Until lineBuffer Is Nothing

				If Not inBody Then

					If lineBuffer <> "" Then
						If Me.ExtractSMTPHeader(lineBuffer) <> "" Then
							If Me.IsValidSMTPHeader(Me.ExtractSMTPHeader(lineBuffer)) Then
								header = Me.ExtractSMTPHeader(lineBuffer)
								If header.ToLower = "date" Then
									headerValue = lineBuffer.Substring(lineBuffer.IndexOf(":") + 1, lineBuffer.IndexOf("-") - lineBuffer.IndexOf(":") - 2)
									Try
										Dim dt As Date = Date.Parse(headerValue)
									Catch ex As System.Exception
										' if the field cannot be parsed then set it as empty
										headerValue = String.Empty
									End Try
								Else
									headerValue = lineBuffer.Substring(lineBuffer.IndexOf(":") + 1)
								End If

							Else
								headerValue = headerValue + lineBuffer
							End If
						Else
							headerValue = headerValue + lineBuffer
						End If

						If hashTable.Contains(header.ToLower) Then
							hashTable.Item(header.ToLower) = headerValue
						Else
							hashTable.Add(header.ToLower, headerValue)
						End If

					Else
						inBody = True
					End If
				Else
					body.Append(lineBuffer)
					body.Append(vbCrLf)
				End If

				lineBuffer = sr.ReadLine
			Loop
			sr.Close()

			hashTable.Add("body", body.ToString)

			Return hashTable

		End Function

		Private Function ExtractSMTPHeader(ByVal lineBuffer As String) As String
			If lineBuffer.IndexOf(":") > -1 Then
				Return lineBuffer.Substring(0, lineBuffer.IndexOf(":"))
			End If
			Return Nothing
		End Function

		Private Function IsValidSMTPHeader(ByVal header As String) As Boolean
			Select Case header.ToLower
				Case "to", "bcc", "cc", "message-id", "x-filename", "x-origin", "x-bcc", "from", "x-to", "x-from", "content-type", "content-transfer-Encoding", "x-cc", "x-folder", "mime-version", "subject", "date"
					Return True
				Case Else
					Return False
			End Select
		End Function

	End Class
End Namespace