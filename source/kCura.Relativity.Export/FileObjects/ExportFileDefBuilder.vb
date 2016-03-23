Imports System.Collections.Generic
Imports System.Net
Imports System.Text
Imports kCura.Relativity.Export.Exports
Imports kCura.Relativity.Export.Types
Imports Relativity

Namespace kCura.Relativity.Export.FileObjects


Public Class ExportFileDefBuilder

		Public Shared Function CreateDefSetup(exportedObjArtifactId As Integer,  workspaceId As Integer, password As String, userName As String, 
											  exportFilesLocation As String, selViewFieldInfos As List(Of Types.ViewFieldInfo), Optional artifactTypeId As Integer = 10) As ExportFile

			Dim expFile As ExportFile = New ExportFile(artifactTypeId)

			'Below are set up in construcotrs
			'expFile.MultiRecordDelimiter =
			'expFile.QuoteDelimiter = 
			'expFile.RecordDelimiter= 
			'expFile.MulticodesAsNested= 
			'expFile.NestedValueDelimiter= 
			'expFile.NewlineDelimiter= 


			'expFile.AllExportableFields = fieldInfos.ToArray()

			expFile.AppendOriginalFileName = False
			expFile.ArtifactID= exportedObjArtifactId

			expFile.CaseInfo = New CaseInfo()
			expFile.CaseInfo.ArtifactID = workspaceId
			'expFile.CaseInfo.AsImportAllowed = False
			'expFile.CaseInfo.DocumentPath
			'expFile.CaseInfo.DownloadHandlerURL
			'expFile.CaseInfo.EnableDataGrid = False
			'expFile.CaseInfoExportAllowed = False
			'expFile.CaseInfo.MatterArtifactID = 
			'expFile.CaseInfo.StatusCodeArtifactID
			

			expFile.CookieContainer =  New System.Net.CookieContainer

			expFile.Credential= New NetworkCredential()
			expFile.Credential.Password = password
			expFile.Credential.UserName = userName

			'expFile.DataTable= 
			expFile.ExportFullText = False

			expFile.ExportImages = True

			expFile.ExportFullTextAsFile = False
			expFile.ExportNative= True

			expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier

			'Not needed for now:
			'If exportFile.FileField IsNot Nothing

			'	expFile.FileField= New DocumentField(exportFile.FileField.FieldName, exportFile.FileField.FieldID, exportFile.FileField.FieldTypeID, exportFile.FileField.FieldCategoryID,
			'										exportFile.FileField.CodeTypeID, exportFile.FileField.FieldLength, exportFile.FileField.AssociatedObjectTypeID, exportFile.FileField.UseUnicode,
			'										exportFile.FileField.Guids, exportFile.FileField.EnableDataGrid)
			'End If

			expFile.FilePrefix = ""
			expFile.FolderPath= exportFilesLocation

			' TODO:
			expFile.IdentifierColumnName= "Control Number"


			'RDC: GetImagePrecedence methods!!!!!!!!!!!

			Dim imagePrecs As List(Of Pair) = new List(Of Pair)
			imagePrecs.Add(new Pair("Original", "-1"))
			
			expFile.ImagePrecedence = imagePrecs.ToArray()
			expFile.LoadFileEncoding = System.Text.Encoding.Default
			expFile.LoadFileExtension= "dat"

			expFile.LoadFileIsHtml= False

			expFile.LoadFilesPrefix = "Extracted Text Only"
			expFile.LogFileFormat = LoadFileType.FileFormat.Opticon
			
			expFile.ObjectTypeName = "Document"
			expFile.Overwrite = True
			

			expFile.RenameFilesToIdentifier = True

			'This is for now not needed:
			'expFile.SelectedTextFields= selFieldInfos.ToArray()
			

			expFile.SelectedViewFields = selViewFieldInfos.ToArray()
			
			
			expFile.StartAtDocumentNumber = 0
			expFile.SubdirectoryDigitPadding = 3
			expFile.TextFileEncoding = Nothing

			expFile.TypeOfExport = ExportFile.ExportType.ArtifactSearch
			
			expFile.TypeOfExportedFilePath = ExportFile.ExportedFilePathType.Relative

			expFile.TypeOfImage = ExportFile.ImageType.SinglePage
			
			expFile.ViewID = 0
			expFile.VolumeDigitPadding= 2

			expFile.VolumeInfo = new VolumeInfo()
			expFile.VolumeInfo.VolumePrefix = "VOL"
			expFile.VolumeInfo.VolumeStartNumber = 1 
			expFile.VolumeInfo.VolumeMaxSize = 650
			expFile.VolumeInfo.SubdirectoryStartNumber = 1
			expFile.VolumeInfo.SubdirectoryMaxSize = 500
			expFile.VolumeInfo.CopyFilesFromRepository = True

			return expFile 
		End Function

End Class


End Namespace
