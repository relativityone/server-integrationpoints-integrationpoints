Imports System.Xml.Linq
Imports kCura.Relativity.Export.Types
Imports Relativity

Namespace kCura.Relativity.Export.FileObjects
	Public Class ExportFileSerializer
		Private _settingsValidator As New ExportSettingsValidator
		Public Property SettingsValidator As ExportSettingsValidator
			Get
				If _settingsValidator Is Nothing Then _settingsValidator = New ExportSettingsValidator
				Return _settingsValidator
			End Get
			Set(ByVal value As ExportSettingsValidator)
				_settingsValidator = value
			End Set
		End Property

		Public Overridable Function TransformExportFileXml(ByVal input As XDocument) As String
			Return input.ToString
		End Function

		Public Overridable Function DeserializeExportFile(ByVal currentExportFile As ExportFile, ByVal xml As String) As ExportFile
			Dim retval As New ExportFile(currentExportFile.ArtifactTypeID)
			Dim deserialized As ExportFile = Me.DeserializeExportFile(XDocument.Parse(xml))
			For Each p As System.Reflection.PropertyInfo In (From prop As System.Reflection.PropertyInfo In retval.GetType.GetProperties Where prop.CanWrite)
				p.SetValue(retval, p.GetValue(If(PropertyIsReadFromExisting(p), currentExportFile, deserialized), Nothing), Nothing)
			Next
			'TODO: test
			Select Case retval.TypeOfExport
				Case ExportFile.ExportType.AncestorSearch, ExportFile.ExportType.ParentSearch
					retval.ArtifactID = currentExportFile.ArtifactID
				Case ExportFile.ExportType.Production
					retval.ImagePrecedence = New Pair() {}
			End Select
			If Not SqlNameHelper.GetSqlFriendlyName(currentExportFile.ObjectTypeName).Equals(SqlNameHelper.GetSqlFriendlyName(retval.ObjectTypeName)) Then
				retval = New ErrorExportFile("Cannot load '" & currentExportFile.ObjectTypeName & "' settings from a saved '" & retval.ObjectTypeName & "' export")
			End If
			If Not Me.SettingsValidator.IsValidExportDirectory(retval.FolderPath) Then retval.FolderPath = String.Empty
			Return retval
		End Function

		Private Function PropertyIsReadFromExisting(ByVal p As System.Reflection.PropertyInfo) As Boolean
			For Each att As Attribute In p.GetCustomAttributes(GetType(ReadFromExisting), False)
				Return True
			Next
			Return False
		End Function

		Public Overridable Function DeserializeExportFile(ByVal xml As XDocument) As ExportFile
			Dim deserializer As New System.Runtime.Serialization.Formatters.Soap.SoapFormatter
			Dim cleansedInput As String = Me.TransformExportFileXml(xml)
			Dim sr As New System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(cleansedInput))
			Dim deserialized As ExportFile = Nothing
			Try
				deserialized = DirectCast(deserializer.Deserialize(sr), ExportFile)
			Catch
				Throw
			Finally
				sr.Close()
			End Try
			Return deserialized
		End Function

		Public Class ExportSettingsValidator
			Public Overridable Function IsValidExportDirectory(ByVal path As String) As Boolean
				Return System.IO.Directory.Exists(path)
			End Function
		End Class
	End Class
End Namespace

