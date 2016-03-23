Imports kCura.Relativity.Export.Exports
Imports kCura.Relativity.Export.FileObjects
Imports Relativity

Namespace kCura.Relativity.Export.Exports

	Public Class HtmlCellFormatter
		Implements ILoadFileCellFormatter
		Private _settings As ExportFile
		Private Const ROW_PREFIX As String = "<tr>"
		Private Const ROW_SUFFIX As String = "</tr>"
		Public Sub New(ByVal settings As ExportFile)
			_settings = settings
		End Sub

		Public Function TransformToCell(ByVal contents As String) As String Implements ILoadFileCellFormatter.TransformToCell
			contents = System.Web.HttpUtility.HtmlEncode(contents)
			Return String.Format("{0}{1}{2}", "<td>", contents, "</td>")
		End Function

		Private Function GetNativeHtmlString(ByVal artifact As ObjectExportInfo, ByVal location As String) As String
			If _settings.ArtifactTypeID = ArtifactType.Document AndAlso artifact.NativeCount = 0 Then Return ""
			If Not _settings.ArtifactTypeID = ArtifactType.Document AndAlso Not artifact.FileID > 0 Then Return ""
			Dim retval As New System.Text.StringBuilder
			retval.AppendFormat("<a style='display:block' href='{0}'>{1}</a>", location, artifact.NativeFileName(_settings.AppendOriginalFileName))
			Return retval.ToString
		End Function

		Public ReadOnly Property RowPrefix() As String Implements ILoadFileCellFormatter.RowPrefix
			Get
				Return ROW_PREFIX
			End Get
		End Property

		Public ReadOnly Property RowSuffix() As String Implements ILoadFileCellFormatter.RowSuffix
			Get
				Return ROW_SUFFIX
			End Get
		End Property

		Private Function GetImagesHtmlString(ByVal artifact As ObjectExportInfo) As String
			If artifact.Images.Count = 0 Then Return ""
			Dim retval As New System.Text.StringBuilder
			For Each image As ImageExportInfo In artifact.Images
				Dim loc As String = image.TempLocation
				If Not _settings.VolumeInfo.CopyFilesFromRepository Then
					loc = image.SourceLocation
				End If
				retval.AppendFormat("<a style='display:block' href='{0}'>{1}</a>", loc, image.FileName)
				If _settings.TypeOfImage = ExportFile.ImageType.MultiPageTiff OrElse _settings.TypeOfImage = ExportFile.ImageType.Pdf Then Exit For
			Next
			Return retval.ToString
		End Function

		Public Function CreateImageCell(ByVal artifact As ObjectExportInfo) As String Implements ILoadFileCellFormatter.CreateImageCell
			If Not _settings.ExportImages OrElse _settings.ArtifactTypeID <> ArtifactType.Document Then Return String.Empty
			Return String.Format("<td>{0}</td>", Me.GetImagesHtmlString(artifact))
		End Function

		Public Function CreateNativeCell(ByVal location As String, ByVal artifact As ObjectExportInfo) As String Implements ILoadFileCellFormatter.CreateNativeCell
			Return String.Format("<td>{0}</td>", Me.GetNativeHtmlString(artifact, location))
		End Function
	End Class
End Namespace

