Imports System.Text
Imports System.Runtime.CompilerServices

Namespace GeneratorHelper
	Public Module ExtensionMethods

		<Extension()> _
		Function ToToken(ByVal value As String, ByVal startUpperCase As Boolean) As String
			Dim result = New StringBuilder()
			Dim precedingWhiteSpace = False
			Dim first = True
			For Each character In value.Trim().ToCharArray()
				If Char.IsWhiteSpace(character) Then
					precedingWhiteSpace = True
				ElseIf Char.IsLetter(character) OrElse (Not first AndAlso Char.IsDigit(character)) Then

					If (result.Length = 0 AndAlso Not startUpperCase) Then
						result.Append(Char.ToLower(character))
					ElseIf (precedingWhiteSpace OrElse result.Length = 0) Then
						result.Append(Char.ToUpper(character))
					Else
						result.Append(character)
					End If
					precedingWhiteSpace = False

				End If
				first = False
			Next
			Return result.ToString()
		End Function


		<Extension()> _
		Function ToCamelCase(ByVal value As String) As String
			Return ToToken(value, False)
		End Function

		<Extension()> _
		Function ToPascalCase(ByVal value As String) As String
			Return ToToken(value, True)
		End Function

	End Module
End Namespace