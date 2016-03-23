Imports kCura.Relativity.Export.FileObjects
Imports kCura.Relativity.Export.Process
Imports kCura.Relativity.Export.Types
Imports Relativity

Imports RelativityConstants = Relativity.Constants


Namespace kCura.Relativity.Export.Exports
	Public Class VolumeManager

#Region "Members"

		Private _settings As ExportFile
		Private _imageFileWriter As System.IO.StreamWriter
		Private _nativeFileWriter As System.IO.StreamWriter
		Private _errorWriter As System.IO.StreamWriter

		Private _nativeFileWriterPosition As Int64 = 0
		Private _imageFileWriterPosition As Int64 = 0
		Private _errorWriterPosition As Int64 = 0

		Private _currentVolumeNumber As Int32
		Private _currentSubdirectoryNumber As Int32

		Private _currentVolumeSize As Int64
		Private _currentImageSubdirectorySize As Int64
		Private _currentNativeSubdirectorySize As Int64
		Private _currentTextSubdirectorySize As Int64

		Private _volumeLabelPaddingWidth As Int32
		Private _subdirectoryLabelPaddingWidth As Int32
		Private _downloadManager As FileDownloader

		Private _parent As Exporter
		Private _columnHeaderString As String
		Private _hasWrittenColumnHeaderString As Boolean = False
		Private _encoding As System.Text.Encoding
		Private _errorFileLocation As String = ""
		Private _timekeeper As kCura.Utility.Timekeeper
		Private _statistics As Statistics
		Private _totalExtractedTextFileLength As Int64 = 0
		Private _halt As Boolean = False
		Private _ordinalLookup As New System.Collections.Generic.Dictionary(Of String, Int32)
		Private _loadFileFormatter As ILoadFileCellFormatter
#End Region

		Private Enum ExportFileType
			Image
			Native
		End Enum

#Region "Accessors"
		Public ReadOnly Property ErrorLogFileName() As String
			Get
				Try
					Return _errorFileLocation
				Catch ex As System.Exception
					Return ""
				End Try
			End Get
		End Property

		Public Property Halt() As Boolean
			Get
				Return _halt
			End Get
			Set(ByVal value As Boolean)
				_halt = value
			End Set
		End Property

		Public ReadOnly Property Settings() As ExportFile
			Get
				Return _settings
			End Get
		End Property

		Private ReadOnly Property CurrentVolumeLabel() As String
			Get
				Return _settings.VolumeInfo.VolumePrefix & _currentVolumeNumber.ToString.PadLeft(_volumeLabelPaddingWidth, "0"c)
			End Get
		End Property

		Private ReadOnly Property CurrentImageSubdirectoryLabel() As String
			Get
				Return _settings.VolumeInfo.SubdirectoryImagePrefix & _currentSubdirectoryNumber.ToString.PadLeft(_subdirectoryLabelPaddingWidth, "0"c)
			End Get
		End Property

		Private ReadOnly Property CurrentNativeSubdirectoryLabel() As String
			Get
				Return _settings.VolumeInfo.SubdirectoryNativePrefix & _currentSubdirectoryNumber.ToString.PadLeft(_subdirectoryLabelPaddingWidth, "0"c)
			End Get
		End Property

		Private ReadOnly Property CurrentFullTextSubdirectoryLabel() As String
			Get
				Return _settings.VolumeInfo.SubdirectoryFullTextPrefix & _currentSubdirectoryNumber.ToString.PadLeft(_subdirectoryLabelPaddingWidth, "0"c)
			End Get
		End Property

		Public ReadOnly Property SubDirectoryMaxSize() As Int64
			Get
				Return Me.Settings.VolumeInfo.SubdirectoryMaxSize
			End Get
		End Property

		Public ReadOnly Property VolumeMaxSize() As Long
			Get
				Return Me.Settings.VolumeInfo.VolumeMaxSize * 1024 * 1024
			End Get
		End Property

		Public Property ColumnHeaderString() As String
			Get
				Return _columnHeaderString
			End Get
			Set(ByVal value As String)
				_columnHeaderString = value
			End Set
		End Property

		Public ReadOnly Property OrdinalLookup() As System.Collections.Generic.Dictionary(Of String, Int32)
			Get
				Return _ordinalLookup
			End Get
		End Property

		Protected Overridable ReadOnly Property NumberOfRetries() As Int32
			Get
				Return kCura.Utility.Config.IOErrorNumberOfRetries
			End Get
		End Property

		Protected Overridable ReadOnly Property WaitTimeBetweenRetryAttempts() As Int32
			Get
				Return kCura.Utility.Config.IOErrorWaitTimeInSeconds
			End Get
		End Property

#End Region

#Region "Constructors"

		Public Sub New(ByVal settings As ExportFile, ByVal rootDirectory As String, ByVal overWriteFiles As Boolean, ByVal totalFiles As Int64, ByVal parent As Exporter, ByVal downloadHandler As FileDownloader, ByVal t As kCura.Utility.Timekeeper, ByVal columnNamesInOrder As String(), ByVal statistics As ExportStatistics)
			_settings = settings
			_statistics = statistics

			If Me.Settings.ExportImages Then
			End If
			_timekeeper = t
			_currentVolumeNumber = _settings.VolumeInfo.VolumeStartNumber
			_currentSubdirectoryNumber = _settings.VolumeInfo.SubdirectoryStartNumber
			Dim volumeNumberPaddingWidth As Int32 = CType(System.Math.Floor(System.Math.Log10(CType(_currentVolumeNumber + 1, Double)) + 1), Int32)
			Dim subdirectoryNumberPaddingWidth As Int32 = CType(System.Math.Floor(System.Math.Log10(CType(_currentSubdirectoryNumber + 1, Double)) + 1), Int32)
			Dim totalFilesNumberPaddingWidth As Int32 = CType(System.Math.Floor(System.Math.Log10(CType(totalFiles + _currentVolumeNumber + 1, Double)) + 1), Int32)
			_volumeLabelPaddingWidth = System.Math.Max(totalFilesNumberPaddingWidth, volumeNumberPaddingWidth)
			totalFilesNumberPaddingWidth = CType(System.Math.Floor(System.Math.Log10(CType(totalFiles + _currentSubdirectoryNumber, Double)) + 1), Int32)
			_subdirectoryLabelPaddingWidth = System.Math.Max(totalFilesNumberPaddingWidth, subdirectoryNumberPaddingWidth)

			'TODO in WEB
			'If Not (_volumeLabelPaddingWidth <= settings.VolumeDigitPadding AndAlso _subdirectoryLabelPaddingWidth <= settings.SubdirectoryDigitPadding) Then
			'	Dim message As New System.Text.StringBuilder
			'	If _volumeLabelPaddingWidth > settings.VolumeDigitPadding Then message.AppendFormat("The selected volume padding of {0} is less than the recommended volume padding {1} for this export" & vbNewLine, settings.VolumeDigitPadding, _volumeLabelPaddingWidth)
			'	If _subdirectoryLabelPaddingWidth > settings.SubdirectoryDigitPadding Then message.AppendFormat("The selected subdirectory padding of {0} is less than the recommended subdirectory padding {1} for this export" & vbNewLine, settings.SubdirectoryDigitPadding, _subdirectoryLabelPaddingWidth)
			'	message.Append("Continue with this selection?")
			'	Select Case MsgBox(message.ToString, MsgBoxStyle.OkCancel, "Relativity Desktop Client")
			'		Case MsgBoxResult.Cancel
			'			parent.Shutdown()
			'			Exit Sub
			'		Case Else
			'			If Not parent.ExportManager.HasExportPermissions(_settings.CaseArtifactID) Then Throw New Service.ExportManager.InsufficientPermissionsForExportException("Export permissions revoked!  Please contact your system administrator to re-instate export permissions.")
			'	End Select
			'End If

			_subdirectoryLabelPaddingWidth = settings.SubdirectoryDigitPadding
			_volumeLabelPaddingWidth = settings.VolumeDigitPadding
			_currentVolumeSize = 0
			_currentImageSubdirectorySize = 0
			_currentTextSubdirectorySize = 0
			_currentNativeSubdirectorySize = 0
			_downloadManager = downloadHandler
			_parent = parent
			Dim loadFilePath As String = Me.LoadFileDestinationPath
			'TODO in Web
			'If Not Me.Settings.Overwrite AndAlso System.IO.File.Exists(loadFilePath) Then
			'	MsgBox(String.Format("Overwrite not selected and file '{0}' exists.", loadFilePath))
			'	_parent.Shutdown()
			'	Exit Sub
			'End If
			'If Me.Settings.ExportImages AndAlso Not Me.Settings.Overwrite AndAlso System.IO.File.Exists(Me.ImageFileDestinationPath) Then
			'	MsgBox(String.Format("Overwrite not selected and file '{0}' exists.", Me.ImageFileDestinationPath))
			'	_parent.Shutdown()
			'	Exit Sub
			'End If
			_encoding = Me.Settings.LoadFileEncoding

			If settings.ExportNative OrElse settings.SelectedViewFields.Length > 0 Then _nativeFileWriter = New System.IO.StreamWriter(loadFilePath, False, _encoding)
			Dim imageFilePath As String = Me.ImageFileDestinationPath
			If Me.Settings.ExportImages Then
				If Not Me.Settings.Overwrite AndAlso System.IO.File.Exists(imageFilePath) Then
					Throw New System.Exception(String.Format("Overwrite not selected and file '{0}' exists.", imageFilePath))
				End If
				_imageFileWriter = New System.IO.StreamWriter(imageFilePath, False, Me.GetImageFileEncoding)
			End If
			If Me.Settings.LoadFileIsHtml Then
				_loadFileFormatter = New HtmlCellFormatter(Me.Settings)
			Else
				_loadFileFormatter = New DelimitedCellFormatter(Me.Settings)
			End If
			For i As Int32 = 0 To columnNamesInOrder.Length - 1
				_ordinalLookup.Add(columnNamesInOrder(i), i)
			Next
			If Not Me.Settings.SelectedTextFields Is Nothing AndAlso Me.Settings.SelectedTextFields.Count > 0 Then
				Dim newindex As Int32 = _ordinalLookup.Count
				_ordinalLookup.Add(Global.Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_ORIGINALSOURCE_AVF_COLUMN_NAME, newindex)
				_ordinalLookup.Add(Global.Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME, newindex + 1)
			End If

		End Sub

		Private Function GetImageFileEncoding() As System.Text.Encoding
			Dim retval As System.Text.Encoding
			If Me.Settings.ExportImages Then
				retval = System.Text.Encoding.Default
				If Me.Settings.LogFileFormat <> LoadFileType.FileFormat.Opticon Then
					retval = System.Text.Encoding.UTF8
				End If
			Else
				retval = _encoding
			End If
			Return retval
		End Function


		Public ReadOnly Property LoadFileDestinationPath() As String
			Get
				Return Me.Settings.FolderPath.TrimEnd("\"c) & "\" & Me.Settings.LoadFilesPrefix & "_export." & Me.Settings.LoadFileExtension
			End Get
		End Property

		Public ReadOnly Property ImageFileDestinationPath() As String
			Get
				Dim logFileExension As String = ""
				Select Case Me.Settings.LogFileFormat
					Case LoadFileType.FileFormat.Opticon
						logFileExension = ".opt"
					Case LoadFileType.FileFormat.IPRO
						logFileExension = ".lfp"
					Case LoadFileType.FileFormat.IPRO_FullText
						logFileExension = "_FULLTEXT_.lfp"
					Case Else
				End Select
				Return Me.Settings.FolderPath.TrimEnd("\"c) & "\" & Me.Settings.LoadFilesPrefix & "_export" & logFileExension
			End Get
		End Property

		Public ReadOnly Property ErrorDestinationPath() As String
			Get
				If String.IsNullOrEmpty(_errorFileLocation) Then _errorFileLocation = System.IO.Path.GetTempFileName()
				Return _errorFileLocation
			End Get
		End Property

		Private Sub LogFileExportError(ByVal type As ExportFileType, ByVal recordIdentifier As String, ByVal fileLocation As String, ByVal errorText As String)
			Try
				If _errorWriter Is Nothing Then
					_errorWriter = New System.IO.StreamWriter(Me.ErrorDestinationPath, False, _encoding)
					_errorWriter.WriteLine("""File Type"",""Document Identifier"",""File Guid"",""Error Description""")
				End If
				_errorWriter.WriteLine(String.Format("""{0}"",""{1}"",""{2}"",""{3}""", type.ToString, recordIdentifier, fileLocation, kCura.Utility.Strings.ToCsvCellContents(errorText)))
			Catch ex As System.IO.IOException
				Throw New Exceptions.FileWriteException(Exceptions.FileWriteException.DestinationFile.Errors, ex)
			End Try
			_parent.WriteError(String.Format("{0} - Document [{1}] - File [{2}] - Error: {3}{4}", type.ToString, recordIdentifier, fileLocation, System.Environment.NewLine, errorText))
		End Sub

		Public Sub Finish()
			If Not _nativeFileWriter Is Nothing Then
				_nativeFileWriter.Flush()
				_nativeFileWriter.Close()
			End If
			If Not _imageFileWriter Is Nothing Then
				_imageFileWriter.Flush()
				_imageFileWriter.Close()
			End If
			If Not _errorWriter Is Nothing Then
				_errorWriter.Flush()
				_errorWriter.Close()
			End If
		End Sub
#End Region

		Public Function ExportArtifact(ByVal artifact As ObjectExportInfo) As Int64
			Dim tries As Int32 = 0
			Dim maxTries As Int32 = NumberOfRetries + 1
			While tries < maxTries And Not Me.Halt
				tries += 1
				Try
					Return Me.ExportArtifact(artifact, tries > 1)
					Exit While
				Catch ex As Exceptions.ExportBaseException
					If tries = maxTries Then Throw
					_parent.WriteWarning(String.Format("Error writing data file(s) for document {0}", artifact.IdentifierValue))
					_parent.WriteWarning(String.Format("Actual error: {0}", ex.ToString))
					If tries > 1 Then
						_parent.WriteWarning(String.Format("Waiting {0} seconds to retry", WaitTimeBetweenRetryAttempts))
						System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
					Else
						_parent.WriteWarning("Retrying now")
					End If
				End Try
			End While
			Return Nothing
		End Function

		Private Sub ReInitializeAllStreams()
			If Not _nativeFileWriter Is Nothing Then _nativeFileWriter = Me.ReInitializeStream(_nativeFileWriter, _nativeFileWriterPosition, Me.LoadFileDestinationPath, _encoding)
			If Not _imageFileWriter Is Nothing Then _imageFileWriter = Me.ReInitializeStream(_imageFileWriter, _imageFileWriterPosition, Me.ImageFileDestinationPath, Me.GetImageFileEncoding)
		End Sub

		Private Function ReInitializeStream(ByVal brokenStream As System.IO.StreamWriter, ByVal position As Int64, ByVal filepath As String, ByVal encoding As System.Text.Encoding) As System.IO.StreamWriter
			If brokenStream Is Nothing Then Return Nothing
			Try
				brokenStream.Close()
			Catch ex As Exception
			End Try
			Try
				brokenStream = Nothing
			Catch ex As Exception
			End Try
			Try
				Dim retval As New System.IO.StreamWriter(filepath, True, encoding)
				retval.BaseStream.Position = position
				Return retval
			Catch ex As System.IO.IOException
				Throw New Exceptions.FileWriteException(Exceptions.FileWriteException.DestinationFile.Generic, ex)
			End Try
		End Function

#Region " Rollup Images "

		Private Sub RollupImages(ByRef imageCount As Long, ByRef successfulRollup As Boolean, ByVal artifact As ObjectExportInfo, ByVal image As ImageExportInfo)
			Dim imageList(artifact.Images.Count - 1) As String
			For i As Int32 = 0 To imageList.Length - 1
				imageList(i) = DirectCast(artifact.Images(i), ImageExportInfo).TempLocation
			Next
			Dim tempLocation As String = Me.Settings.FolderPath.TrimEnd("\"c) & "\" & System.Guid.NewGuid.ToString & ".tmp"
			Dim converter As New kCura.Utility.Image
			Try
				Select Case Me.Settings.TypeOfImage
					Case ExportFile.ImageType.MultiPageTiff
						converter.ConvertTIFFsToMultiPage(imageList, tempLocation)
					Case ExportFile.ImageType.Pdf
						If Not tempLocation Is Nothing AndAlso Not tempLocation = "" Then converter.ConvertImagesToMultiPagePdf(imageList, tempLocation)
				End Select
				imageCount = 1
				For Each imageLocation As String In imageList
					kCura.Utility.File.Instance.Delete(imageLocation)
				Next
				Dim ext As String = ""
				Select Case Me.Settings.TypeOfImage
					Case ExportFile.ImageType.Pdf
						ext = ".pdf"
					Case ExportFile.ImageType.MultiPageTiff
						ext = ".tif"
				End Select
				Dim currentTempLocation As String = Me.GetImageExportLocation(image)
				If currentTempLocation.IndexOf("."c) <> -1 Then currentTempLocation = currentTempLocation.Substring(0, currentTempLocation.LastIndexOf("."))
				currentTempLocation &= ext
				DirectCast(artifact.Images(0), ImageExportInfo).TempLocation = currentTempLocation
				currentTempLocation = DirectCast(artifact.Images(0), ImageExportInfo).FileName
				If currentTempLocation.IndexOf("."c) <> -1 Then currentTempLocation = currentTempLocation.Substring(0, currentTempLocation.LastIndexOf("."))
				currentTempLocation &= ext
				DirectCast(artifact.Images(0), ImageExportInfo).FileName = currentTempLocation
				Dim location As String = DirectCast(artifact.Images(0), ImageExportInfo).TempLocation
				If System.IO.File.Exists(DirectCast(artifact.Images(0), ImageExportInfo).TempLocation) Then
					If Me.Settings.Overwrite Then
						kCura.Utility.File.Instance.Delete(DirectCast(artifact.Images(0), ImageExportInfo).TempLocation)
						kCura.Utility.File.Instance.Move(tempLocation, DirectCast(artifact.Images(0), ImageExportInfo).TempLocation)
					Else
						_parent.WriteWarning("File exists - file copy skipped: " & DirectCast(artifact.Images(0), ImageExportInfo).TempLocation)
					End If
				Else
					kCura.Utility.File.Instance.Move(tempLocation, DirectCast(artifact.Images(0), ImageExportInfo).TempLocation)
				End If
			Catch ex As kCura.Utility.Image.ImageRollupException
				successfulRollup = False
				Try
					If Not tempLocation Is Nothing AndAlso Not tempLocation = "" Then kCura.Utility.File.Instance.Delete(tempLocation)
					_parent.WriteImgProgressError(artifact, ex.ImageIndex, ex, "Document exported in single-page image mode.")
				Catch ioex As System.IO.IOException
					Throw New Exceptions.FileWriteException(Exceptions.FileWriteException.DestinationFile.Errors, ioex)
				End Try
			End Try
		End Sub

#End Region

		Private Function CopySelectedLongTextToFile(ByVal artifact As ObjectExportInfo, ByRef len As Int64) As String
			Dim field As Types.ViewFieldInfo = Me.GetFieldForLongTextPrecedenceDownload(Nothing, artifact)
			If Not Me.OrdinalLookup.ContainsKey(Global.Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME) Then
				Return String.Empty
			End If
			Dim text As Object = artifact.Metadata(Me.OrdinalLookup(Global.Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME))
			If text Is Nothing Then text = String.Empty
			Dim longText As String = text.ToString
			If longText = RelativityConstants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN Then
				Dim filePath As String = Me.DownloadTextFieldAsFile(artifact, field)
				len += New System.IO.FileInfo(filePath).Length
				Return filePath
			Else
				len += longText.Length
				Return String.Empty
			End If
		End Function

		Private Function TextPrecedenceIsSet() As Boolean
			If Me.Settings.SelectedTextFields Is Nothing Then Return False
			If Me.Settings.SelectedTextFields.Count = 0 Then Return False
			Return (From f In Me.Settings.SelectedTextFields Where f IsNot Nothing).Any
		End Function

		Private Function ExportArtifact(ByVal artifact As ObjectExportInfo, ByVal isRetryAttempt As Boolean) As Int64
			If isRetryAttempt Then Me.ReInitializeAllStreams()
			Dim totalFileSize As Int64 = 0
			Dim loadFileBytes As Int64 = 0
			Dim extractedTextFileSizeForVolume As Int64 = 0
			Dim image As ImageExportInfo = Nothing
			Dim imageSuccess As Boolean = True
			Dim nativeSuccess As Boolean = True
			Dim updateVolumeAfterExport As Boolean = False
			Dim updateSubDirectoryAfterExport As Boolean = False
			If Me.Settings.ExportImages Then
				For Each image In artifact.Images
					_timekeeper.MarkStart("VolumeManager_DownloadImage")
					Try
						If Me.Settings.VolumeInfo.CopyFilesFromRepository Then
							totalFileSize += Me.DownloadImage(image)
						End If
						image.HasBeenCounted = True
					Catch ex As System.Exception
						image.TempLocation = ""
						Me.LogFileExportError(ExportFileType.Image, artifact.IdentifierValue, image.FileGuid, ex.ToString)
						imageSuccess = False
					End Try
					_timekeeper.MarkEnd("VolumeManager_DownloadImage")
				Next
			End If
			Dim imageCount As Long = artifact.Images.Count
			Dim successfulRollup As Boolean = True
			If artifact.Images.Count > 0 AndAlso (Me.Settings.TypeOfImage = ExportFile.ImageType.MultiPageTiff OrElse Me.Settings.TypeOfImage = ExportFile.ImageType.Pdf) Then
				Me.RollupImages(imageCount, successfulRollup, artifact, image)
			End If

			If Me.Settings.ExportNative Then
				_timekeeper.MarkStart("VolumeManager_DownloadNative")
				Try
					If Me.Settings.VolumeInfo.CopyFilesFromRepository Then
						Dim downloadSize As Int64 = Me.DownloadNative(artifact)
						If Not artifact.HasCountedNative Then totalFileSize += downloadSize
					End If
					artifact.HasCountedNative = True
				Catch ex As System.Exception
					Me.LogFileExportError(ExportFileType.Native, artifact.IdentifierValue, artifact.NativeFileGuid, ex.ToString)
				End Try
				_timekeeper.MarkEnd("VolumeManager_DownloadNative")
			End If
			Dim tempLocalFullTextFilePath As String = ""
			Dim tempLocalIproFullTextFilePath As String = ""

			Dim extractedTextFileLength As Long = 0
			If Me.Settings.ExportFullText AndAlso Me.Settings.ExportFullTextAsFile Then
				Dim len As Int64 = 0
				tempLocalFullTextFilePath = Me.CopySelectedLongTextToFile(artifact, len)
				If Me.Settings.ExportFullTextAsFile Then
					If Not artifact.HasCountedTextFile Then
						totalFileSize += len
						extractedTextFileSizeForVolume += len
					End If
					artifact.HasCountedTextFile = True
				End If
				artifact.HasFullText = True
			End If

			If Me.Settings.LogFileFormat = LoadFileType.FileFormat.IPRO_FullText AndAlso Me.Settings.ExportImages Then
				If Not Me.TextPrecedenceIsSet Then
					tempLocalIproFullTextFilePath = System.IO.Path.GetTempFileName
					Dim tries As Int32 = 0
					Dim maxTries As Int32 = NumberOfRetries + 1
					Dim start As Int64 = System.DateTime.Now.Ticks
					'BigData_ET_1037768
					Dim val As String = artifact.Metadata(Me.OrdinalLookup("ExtractedText")).ToString
					If val <> RelativityConstants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN Then
						Dim sw As New System.IO.StreamWriter(tempLocalIproFullTextFilePath, False, System.Text.Encoding.Unicode)
						sw.Write(val)
						sw.Close()
					Else
						While tries < maxTries AndAlso Not Me.Halt
							tries += 1
							Try
								_downloadManager.DownloadFullTextFile(tempLocalIproFullTextFilePath, artifact.ArtifactID, _settings.CaseInfo.ArtifactID.ToString)
								Exit While
							Catch ex As System.Exception
								If tries = 1 Then
									_parent.WriteStatusLine(Windows.Process.EventType.Warning, "Second attempt to download full text for document " & artifact.IdentifierValue, True)
								ElseIf tries < maxTries Then
									Dim waitTime As Int32 = WaitTimeBetweenRetryAttempts
									_parent.WriteStatusLine(Windows.Process.EventType.Warning, "Additional attempt to download full text for document " & artifact.IdentifierValue & " failed - retrying in " & waitTime.ToString() & " seconds", True)
									System.Threading.Thread.CurrentThread.Join(waitTime * 1000)
								Else
									Throw
								End If
							End Try
						End While
					End If
					_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1)
				Else
					If tempLocalFullTextFilePath <> String.Empty Then
						tempLocalIproFullTextFilePath = String.Copy(tempLocalFullTextFilePath)
					Else
						tempLocalIproFullTextFilePath = System.IO.Path.GetTempFileName
						Dim sw As New System.IO.StreamWriter(tempLocalIproFullTextFilePath, False, System.Text.Encoding.Unicode)
						Dim val As String = artifact.Metadata(Me.OrdinalLookup(Global.Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME)).ToString
						sw.Write(val)
						sw.Close()
					End If
				End If
			End If

			Dim textCount As Int32 = 0
			If Me.Settings.ExportFullTextAsFile AndAlso artifact.HasFullText Then textCount += 1
			If totalFileSize + _currentVolumeSize > Me.VolumeMaxSize Then
				If _currentVolumeSize = 0 Then
					updateVolumeAfterExport = True
				Else
					Me.UpdateVolume()
				End If
			ElseIf imageCount + _currentImageSubdirectorySize > Me.SubDirectoryMaxSize Then
				Me.UpdateSubdirectory()
			ElseIf artifact.NativeCount + _currentNativeSubdirectorySize > Me.SubDirectoryMaxSize Then
				Me.UpdateSubdirectory()
			ElseIf textCount + _currentTextSubdirectorySize > Me.SubDirectoryMaxSize Then
				Me.UpdateSubdirectory()
			End If
			If Me.Settings.ExportImages Then
				_timekeeper.MarkStart("VolumeManager_ExportImages")
				Me.ExportImages(artifact.Images, tempLocalIproFullTextFilePath, successfulRollup)
				_timekeeper.MarkEnd("VolumeManager_ExportImages")
			End If
			Dim nativeCount As Int32 = 0
			Dim nativeLocation As String = ""
			If Me.Settings.ExportNative AndAlso Me.Settings.VolumeInfo.CopyFilesFromRepository Then
				Dim nativeFileName As String = Me.GetNativeFileName(artifact)
				Dim localFilePath As String = Me.GetLocalNativeFilePath(artifact, nativeFileName)
				_timekeeper.MarkStart("VolumeManager_ExportNative")
				Me.ExportNative(localFilePath, artifact.NativeFileGuid, artifact.ArtifactID, nativeFileName, artifact.NativeTempLocation)
				_timekeeper.MarkEnd("VolumeManager_ExportNative")
				If artifact.NativeTempLocation = "" Then
					nativeLocation = ""
				Else
					nativeCount = 1
					Select Case Me.Settings.TypeOfExportedFilePath
						Case ExportFile.ExportedFilePathType.Absolute
							nativeLocation = localFilePath
						Case ExportFile.ExportedFilePathType.Relative
							nativeLocation = ".\" & Me.CurrentVolumeLabel & "\" & Me.CurrentNativeSubdirectoryLabel & "\" & nativeFileName
						Case ExportFile.ExportedFilePathType.Prefix
							nativeLocation = Me.Settings.FilePrefix.TrimEnd("\"c) & "\" & Me.CurrentVolumeLabel & "\" & Me.CurrentNativeSubdirectoryLabel & "\" & nativeFileName
					End Select
				End If
			End If
			Try
				If Not _hasWrittenColumnHeaderString AndAlso Not _nativeFileWriter Is Nothing Then
					_nativeFileWriter.Write(_columnHeaderString)
					_hasWrittenColumnHeaderString = True
				End If
				Me.UpdateLoadFile(artifact.Metadata, artifact.HasFullText, artifact.ArtifactID, nativeLocation, tempLocalFullTextFilePath, artifact, extractedTextFileLength)
			Catch ex As System.IO.IOException
				Throw New Exceptions.FileWriteException(Exceptions.FileWriteException.DestinationFile.Load, ex)
			End Try

			_parent.DocumentsExported += artifact.DocCount
			_currentVolumeSize += totalFileSize
			If Me.Settings.VolumeInfo.CopyFilesFromRepository Then
				_currentNativeSubdirectorySize += artifact.NativeCount
				If Me.Settings.ExportFullTextAsFile AndAlso artifact.HasFullText Then _currentTextSubdirectorySize += 1
				_currentImageSubdirectorySize += imageCount
			End If
			If updateSubDirectoryAfterExport Then Me.UpdateSubdirectory()
			If updateVolumeAfterExport Then Me.UpdateVolume()
			_parent.WriteUpdate("Document " & artifact.IdentifierValue & " exported.", False)

			Try
				If Not _nativeFileWriter Is Nothing Then _nativeFileWriter.Flush()
			Catch ex As Exception
				Throw New Exceptions.FileWriteException(Exceptions.FileWriteException.DestinationFile.Load, ex)
			End Try
			Try
				If Not _imageFileWriter Is Nothing Then _imageFileWriter.Flush()
			Catch ex As Exception
				Throw New Exceptions.FileWriteException(Exceptions.FileWriteException.DestinationFile.Image, ex)
			End Try
			Try
				If Not _errorWriter Is Nothing Then _errorWriter.Flush()
			Catch ex As Exception
				Throw New Exceptions.FileWriteException(Exceptions.FileWriteException.DestinationFile.Errors, ex)
			End Try
			If Not _nativeFileWriter Is Nothing Then
				_nativeFileWriterPosition = _nativeFileWriter.BaseStream.Position
				loadFileBytes += kCura.Utility.File.Instance.GetFileSize(DirectCast(_nativeFileWriter.BaseStream, System.IO.FileStream).Name)
			End If
			If Not _imageFileWriter Is Nothing Then
				_imageFileWriterPosition = _imageFileWriter.BaseStream.Position
				loadFileBytes += kCura.Utility.File.Instance.GetFileSize(DirectCast(_imageFileWriter.BaseStream, System.IO.FileStream).Name)
			End If
			_totalExtractedTextFileLength += extractedTextFileLength
			_statistics.MetadataBytes = loadFileBytes + _totalExtractedTextFileLength
			_statistics.FileBytes += totalFileSize - extractedTextFileSizeForVolume
			If Not _errorWriter Is Nothing Then _errorWriterPosition = _errorWriter.BaseStream.Position
			Dim deletor As New TempTextFileDeletor(New String() {tempLocalIproFullTextFilePath, tempLocalFullTextFilePath})
			Dim t As New System.Threading.Thread(AddressOf deletor.DeleteFiles)
			t.Start()
			If Not Me.Settings.VolumeInfo.CopyFilesFromRepository Then
				Return 0
			Else
				Return imageCount + nativeCount
			End If
		End Function

		Private Function GetFieldForLongTextPrecedenceDownload(ByVal input As Types.ViewFieldInfo, ByVal artifact As ObjectExportInfo) As Types.ViewFieldInfo
			Dim retval As Types.ViewFieldInfo = input
			If input Is Nothing OrElse input.AvfColumnName = Global.Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME Then
				If Me.Settings.SelectedTextFields IsNot Nothing Then
					retval = (From f In Me.Settings.SelectedTextFields Where f.FieldArtifactId = CInt(artifact.Metadata(_ordinalLookup(Global.Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_ORIGINALSOURCE_AVF_COLUMN_NAME)))).First
				End If
			End If
			Return retval
		End Function
		Private Function DownloadTextFieldAsFile(ByVal artifact As ObjectExportInfo, ByVal field As Types.ViewFieldInfo) As String
			Dim tempLocalFullTextFilePath As String = System.IO.Path.GetTempFileName
			Dim tries As Int32 = 0
			Dim maxTries As Int32 = NumberOfRetries + 1
			Dim start As Int64 = System.DateTime.Now.Ticks
			While tries < maxTries AndAlso Not Me.Halt
				tries += 1
				Try
					If Me.Settings.ArtifactTypeID = ArtifactType.Document AndAlso field.Category = FieldCategory.FullText AndAlso Not TypeOf field Is CoalescedTextViewField Then
						_downloadManager.DownloadFullTextFile(tempLocalFullTextFilePath, artifact.ArtifactID, _settings.CaseInfo.ArtifactID.ToString)
					Else
						Dim fieldToActuallyExportFrom As Types.ViewFieldInfo = Me.GetFieldForLongTextPrecedenceDownload(field, artifact)
						_downloadManager.DownloadLongTextFile(tempLocalFullTextFilePath, artifact.ArtifactID, fieldToActuallyExportFrom, _settings.CaseInfo.ArtifactID.ToString)
					End If
					Exit While
				Catch ex As System.Exception
					If tries = 1 Then
						_parent.WriteStatusLine(Windows.Process.EventType.Warning, "Second attempt to download full text for document " & artifact.IdentifierValue, True)
					ElseIf tries < maxTries Then
						Dim waitTime As Int32 = WaitTimeBetweenRetryAttempts
						_parent.WriteStatusLine(Windows.Process.EventType.Warning, "Additional attempt to download full text for document " & artifact.IdentifierValue & " failed - retrying in " & waitTime.ToString() & " seconds", True)
						System.Threading.Thread.CurrentThread.Join(waitTime * 1000)
					Else
						Throw
					End If
				End Try
			End While
			_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1)
			Return tempLocalFullTextFilePath
		End Function

		Private Function GetLocalNativeFilePath(ByVal doc As ObjectExportInfo, ByVal nativeFileName As String) As String
			Dim localFilePath As String = Me.Settings.FolderPath
			If localFilePath.Chars(localFilePath.Length - 1) <> "\"c Then localFilePath &= "\"
			localFilePath &= Me.CurrentVolumeLabel & "\" & Me.CurrentNativeSubdirectoryLabel & "\"
			If Not System.IO.Directory.Exists(localFilePath) Then System.IO.Directory.CreateDirectory(localFilePath)
			Return localFilePath & nativeFileName
		End Function

		Private Function GetLocalTextFilePath(ByVal doc As ObjectExportInfo) As String
			Dim localFilePath As String = Me.Settings.FolderPath
			If localFilePath.Chars(localFilePath.Length - 1) <> "\"c Then localFilePath &= "\"
			localFilePath &= Me.CurrentVolumeLabel & "\" & Me.CurrentFullTextSubdirectoryLabel & "\"
			If Not System.IO.Directory.Exists(localFilePath) Then System.IO.Directory.CreateDirectory(localFilePath)
			Return localFilePath & doc.FullTextFileName(Me.NameTextFilesAfterIdentifier)
		End Function

		Private Function GetNativeFileName(ByVal doc As ObjectExportInfo) As String
			Select Case _parent.ExportNativesToFileNamedFrom
				Case ExportNativeWithFilenameFrom.Identifier
					Return doc.NativeFileName(Me.Settings.AppendOriginalFileName)
				Case ExportNativeWithFilenameFrom.Production
					Return doc.ProductionBeginBatesFileName(Me.Settings.AppendOriginalFileName)
			End Select
			Return Nothing
		End Function

		Public Sub Close()
			If Not _imageFileWriter Is Nothing Then
				_imageFileWriter.Flush()
				_imageFileWriter.Close()
			End If
			If Not _nativeFileWriter Is Nothing Then
				_nativeFileWriter.Flush()
				_nativeFileWriter.Close()
			End If
		End Sub

#Region "Image Export"

		Private Function GetImageExportLocation(ByVal image As ImageExportInfo) As String
			Dim localFilePath As String = Me.Settings.FolderPath
			Dim subfolderPath As String = Me.CurrentVolumeLabel & "\" & Me.CurrentImageSubdirectoryLabel & "\"
			If localFilePath.Chars(localFilePath.Length - 1) <> "\"c Then localFilePath &= "\"
			localFilePath &= subfolderPath
			If Not System.IO.Directory.Exists(localFilePath) Then System.IO.Directory.CreateDirectory(localFilePath)
			Return localFilePath & image.FileName
		End Function


		Public Sub ExportImages(ByVal images As System.Collections.ArrayList, ByVal localFullTextPath As String, ByVal successfulRollup As Boolean)
			Dim image As ImageExportInfo
			Dim i As Int32 = 0
			Dim fullTextReader As System.IO.StreamReader = Nothing
			Dim localFilePath As String = Me.Settings.FolderPath
			Dim subfolderPath As String = Me.CurrentVolumeLabel & "\" & Me.CurrentImageSubdirectoryLabel & "\"
			Dim pageOffset As Long
			If localFilePath.Chars(localFilePath.Length - 1) <> "\"c Then localFilePath &= "\"
			localFilePath &= subfolderPath
			If Not System.IO.Directory.Exists(localFilePath) AndAlso Me.Settings.VolumeInfo.CopyFilesFromRepository Then System.IO.Directory.CreateDirectory(localFilePath)
			Try
				If Me.Settings.LogFileFormat = LoadFileType.FileFormat.IPRO_FullText Then
					If System.IO.File.Exists(localFullTextPath) Then
						fullTextReader = New System.IO.StreamReader(localFullTextPath, _encoding, True)
					End If
				End If
				If images.Count > 0 AndAlso (Me.Settings.TypeOfImage = ExportFile.ImageType.MultiPageTiff OrElse Me.Settings.TypeOfImage = ExportFile.ImageType.Pdf) AndAlso successfulRollup Then
					Dim marker As ImageExportInfo = DirectCast(images(0), ImageExportInfo)
					Me.ExportDocumentImage(localFilePath & marker.FileName, marker.FileGuid, marker.ArtifactID, marker.BatesNumber, marker.TempLocation)
					Dim copyfile As String = Nothing
					Select Case Me.Settings.TypeOfExportedFilePath
						Case ExportFile.ExportedFilePathType.Absolute
							copyfile = localFilePath & marker.FileName
						Case ExportFile.ExportedFilePathType.Relative
							copyfile = ".\" & subfolderPath & marker.FileName
						Case ExportFile.ExportedFilePathType.Prefix
							copyfile = Me.Settings.FilePrefix.TrimEnd("\"c) & "\" & subfolderPath & marker.FileName
					End Select
					If Me.Settings.LogFileFormat = LoadFileType.FileFormat.Opticon Then
						Me.CreateImageLogEntry(marker.BatesNumber, copyfile, localFilePath, 1, fullTextReader, localFullTextPath <> "", Int64.MinValue, images.Count)
					Else
						For j As Int32 = 0 To images.Count - 1
							If (j = 0 AndAlso DirectCast(images(j), ImageExportInfo).PageOffset Is Nothing) OrElse j = images.Count - 1 Then
								pageOffset = Int64.MinValue
							Else
								Dim nextImage As ImageExportInfo = DirectCast(images(j + 1), ImageExportInfo)
								If nextImage.PageOffset Is Nothing Then
									pageOffset = Int64.MinValue
								Else
									pageOffset = nextImage.PageOffset.Value
								End If
							End If
							image = DirectCast(images(j), ImageExportInfo)
							Me.CreateImageLogEntry(image.BatesNumber, copyfile, localFilePath, j + 1, fullTextReader, localFullTextPath <> "", pageOffset, images.Count)
						Next
					End If
					marker.TempLocation = copyfile
				Else
					For Each image In images
						If (i = 0 AndAlso image.PageOffset Is Nothing) OrElse i = images.Count - 1 Then
							pageOffset = Int64.MinValue
						Else
							Dim nextImage As ImageExportInfo = DirectCast(images(i + 1), ImageExportInfo)
							If nextImage.PageOffset Is Nothing Then
								pageOffset = Int64.MinValue
							Else
								pageOffset = nextImage.PageOffset.Value
							End If
						End If
						If Me.Settings.VolumeInfo.CopyFilesFromRepository Then
							Me.ExportDocumentImage(localFilePath & image.FileName, image.FileGuid, image.ArtifactID, image.BatesNumber, image.TempLocation)
							Dim copyfile As String = Nothing
							Select Case Me.Settings.TypeOfExportedFilePath
								Case ExportFile.ExportedFilePathType.Absolute
									copyfile = localFilePath & image.FileName
								Case ExportFile.ExportedFilePathType.Relative
									copyfile = ".\" & subfolderPath & image.FileName
								Case ExportFile.ExportedFilePathType.Prefix
									copyfile = Me.Settings.FilePrefix.TrimEnd("\"c) & "\" & subfolderPath & image.FileName
							End Select
							Me.CreateImageLogEntry(image.BatesNumber, copyfile, localFilePath, i + 1, fullTextReader, localFullTextPath <> "", pageOffset, images.Count)
							image.TempLocation = copyfile
						Else
							Me.CreateImageLogEntry(image.BatesNumber, image.SourceLocation, image.SourceLocation, i + 1, fullTextReader, localFullTextPath <> "", pageOffset, images.Count)
						End If
						i += 1
					Next
				End If

			Catch ex As System.Exception
				If Not fullTextReader Is Nothing Then fullTextReader.Close()
				Throw
			End Try
			If Not fullTextReader Is Nothing Then fullTextReader.Close()
		End Sub

		Private Function DownloadImage(ByVal image As ImageExportInfo) As Int64
			If image.FileGuid = "" Then Return 0
			Dim start As Int64 = System.DateTime.Now.Ticks
			Dim tempFile As String = Me.GetImageExportLocation(image)
			'If Me.Settings.TypeOfImage = ExportFile.ImageType.Pdf Then
			'	tempFile = System.IO.Path.GetTempFileName
			'	kCura.Utility.File.Instance.Delete(tempFile)
			'End If
			If System.IO.File.Exists(tempFile) Then
				If _settings.Overwrite Then
					kCura.Utility.File.Instance.Delete(tempFile)
					_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, String.Format("Overwriting image for {0}.", image.BatesNumber), False)
				Else
					_parent.WriteWarning(String.Format("{0} already exists. Skipping file export.", tempFile))
					Return 0
				End If
			End If
			Dim tries As Int32 = 0
			Dim maxTries As Int32 = NumberOfRetries + 1
			While tries < maxTries AndAlso Not Me.Halt
				tries += 1
				Try
					_downloadManager.DownloadFileForDocument(tempFile, image.FileGuid, image.SourceLocation, image.ArtifactID, _settings.CaseArtifactID.ToString)
					image.TempLocation = tempFile
					Exit While
				Catch ex As System.Exception
					If tries = 1 Then
						_parent.WriteStatusLine(Windows.Process.EventType.Warning, "Second attempt to download image " & image.BatesNumber & " - exact error: " & ex.ToString, True)
					ElseIf tries < maxTries Then
						Dim waitTime As Int32 = WaitTimeBetweenRetryAttempts
						_parent.WriteStatusLine(Windows.Process.EventType.Warning, "Additional attempt to download image " & image.BatesNumber & " failed - retrying in " & waitTime.ToString() & " seconds - exact error: " & ex.ToString, True)
						System.Threading.Thread.CurrentThread.Join(waitTime * 1000)
					Else
						Throw
					End If
				End Try
			End While
			_statistics.FileTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1)
			Return kCura.Utility.File.Instance.Length(tempFile)
		End Function

		Private Sub ExportDocumentImage(ByVal fileName As String, ByVal fileGuid As String, ByVal artifactID As Int32, ByVal batesNumber As String, ByVal tempFileLocation As String)
			If Not tempFileLocation = "" AndAlso Not tempFileLocation.ToLower = fileName.ToLower Then
				If System.IO.File.Exists(fileName) Then
					If _settings.Overwrite Then
						kCura.Utility.File.Instance.Delete(fileName)
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, String.Format("Overwriting document image {0}.", batesNumber), False)
						kCura.Utility.File.Instance.Move(tempFileLocation, fileName)
					Else
						_parent.WriteWarning(String.Format("{0}.tif already exists. Skipping file export.", batesNumber))
					End If
				Else
					_timekeeper.MarkStart("VolumeManager_ExportDocumentImage_WriteStatus")
					_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, String.Format("Now exporting document image {0}.", batesNumber), False)
					_timekeeper.MarkEnd("VolumeManager_ExportDocumentImage_WriteStatus")
					_timekeeper.MarkStart("VolumeManager_ExportDocumentImage_MoveFile")
					kCura.Utility.File.Instance.Move(tempFileLocation, fileName)
					_timekeeper.MarkEnd("VolumeManager_ExportDocumentImage_MoveFile")
				End If
				_timekeeper.MarkStart("VolumeManager_ExportDocumentImage_WriteStatus")
				_parent.WriteStatusLine(Windows.Process.EventType.Status, String.Format("Finished exporting document image {0}.", batesNumber), False)
				_timekeeper.MarkEnd("VolumeManager_ExportDocumentImage_WriteStatus")
			End If
			'_parent.DocumentsExported += 1
		End Sub

		Private Function GetLfpFullTextTransform(ByVal c As Char) As String
			Select Case c
				Case ChrW(10), " "c
					Return "|0|0|0|0^"
				Case ","c
					Return ""
				Case Else
					Return c.ToString
			End Select
		End Function

		Private Sub CreateImageLogEntry(ByVal batesNumber As String, ByVal copyFile As String, ByVal pathToImage As String, ByVal pageNumber As Int32, ByVal fullTextReader As System.IO.StreamReader, ByVal expectingTextForPage As Boolean, ByVal pageOffset As Long, ByVal numberOfImages As Int32)
			Try
				Select Case _settings.LogFileFormat
					Case LoadFileType.FileFormat.Opticon
						Me.WriteOpticonLine(batesNumber, pageNumber = 1, copyFile, numberOfImages)
					Case LoadFileType.FileFormat.IPRO
						Me.WriteIproImageLine(batesNumber, pageNumber, copyFile)
					Case LoadFileType.FileFormat.IPRO_FullText
						Dim currentPageFirstByteNumber As Long
						If fullTextReader Is Nothing Then
							If pageNumber = 1 AndAlso expectingTextForPage Then _parent.WriteWarning(String.Format("Could not retrieve full text for document '{0}'", batesNumber))
						Else
							If pageNumber = 1 Then
								currentPageFirstByteNumber = 0
							Else
								currentPageFirstByteNumber = fullTextReader.BaseStream.Position
							End If
							_imageFileWriter.Write("FT,")
							_imageFileWriter.Write(batesNumber)
							_imageFileWriter.Write(",1,1,")
							Select Case pageOffset
								Case Int64.MinValue
									Dim c As Int32 = fullTextReader.Read
									While c <> -1
										_imageFileWriter.Write(Me.GetLfpFullTextTransform(ChrW(c)))
										c = fullTextReader.Read
									End While
								Case Else
									Dim i As Int32 = 0
									Dim c As Int32 = fullTextReader.Read
									While i < pageOffset AndAlso c <> -1
										_imageFileWriter.Write(Me.GetLfpFullTextTransform(ChrW(c)))
										c = fullTextReader.Read
										i += 1
									End While
							End Select
							_imageFileWriter.Write(vbNewLine)
						End If
						_imageFileWriter.Flush()
						Me.WriteIproImageLine(batesNumber, pageNumber, copyFile)
				End Select
			Catch ex As System.IO.IOException
				Throw New Exceptions.FileWriteException(Exceptions.FileWriteException.DestinationFile.Image, ex)
			End Try
		End Sub

		Private Sub WriteIproImageLine(ByVal batesNumber As String, ByVal pageNumber As Int32, ByVal fullFilePath As String)
			Dim linefactory As New LineFactory.SimpleIproImageLineFactory(batesNumber, pageNumber, fullFilePath, Me.CurrentVolumeLabel, Me.Settings.TypeOfImage.Value)
			linefactory.WriteLine(_imageFileWriter)
		End Sub

		Private Sub WriteOpticonLine(ByVal batesNumber As String, ByVal firstDocument As Boolean, ByVal copyFile As String, ByVal imageCount As Int32)
			Dim log As New System.Text.StringBuilder
			log.AppendFormat("{0},{1},{2},", batesNumber, Me.CurrentVolumeLabel, copyFile)
			If firstDocument Then log.Append("Y")
			log.Append(",,,")
			If firstDocument Then log.Append(imageCount)
			_imageFileWriter.WriteLine(log.ToString)
		End Sub
#End Region

		Private Function ExportNative(ByVal exportFileName As String, ByVal fileGuid As String, ByVal artifactID As Int32, ByVal systemFileName As String, ByVal tempLocation As String) As String
			If Not tempLocation = "" AndAlso Not tempLocation.ToLower = exportFileName.ToLower AndAlso Me.Settings.VolumeInfo.CopyFilesFromRepository Then
				If System.IO.File.Exists(exportFileName) Then
					If _settings.Overwrite Then
						kCura.Utility.File.Instance.Delete(exportFileName)
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, String.Format("Overwriting document {0}.", systemFileName), False)
						kCura.Utility.File.Instance.Move(tempLocation, exportFileName)
					Else
						_parent.WriteWarning(String.Format("{0} already exists. Skipping file export.", systemFileName))
					End If
				Else
					_timekeeper.MarkStart("VolumeManager_ExportNative_WriteStatus")
					_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, String.Format("Now exporting document {0}.", systemFileName), False)
					_timekeeper.MarkEnd("VolumeManager_ExportNative_WriteStatus")
					_timekeeper.MarkStart("VolumeManager_ExportNative_MoveFile")
					kCura.Utility.File.Instance.Move(tempLocation, exportFileName)
					_timekeeper.MarkEnd("VolumeManager_ExportNative_MoveFile")
				End If
			End If
			_timekeeper.MarkStart("VolumeManager_ExportNative_WriteStatus")
			_timekeeper.MarkEnd("VolumeManager_ExportNative_WriteStatus")
			Return Nothing
		End Function

		Private Function DownloadNative(ByVal artifact As ObjectExportInfo) As Int64
			If Me.Settings.ArtifactTypeID = ArtifactType.Document AndAlso artifact.NativeFileGuid = "" Then Return 0
			If Not Me.Settings.ArtifactTypeID = ArtifactType.Document AndAlso (Not artifact.FileID > 0 OrElse artifact.NativeSourceLocation.Trim = String.Empty) Then Return 0
			Dim nativeFileName As String = Me.GetNativeFileName(artifact)
			Dim tempFile As String = Me.GetLocalNativeFilePath(artifact, nativeFileName)
			Dim start As Int64 = System.DateTime.Now.Ticks
			If System.IO.File.Exists(tempFile) Then
				If Settings.Overwrite Then
					kCura.Utility.File.Instance.Delete(tempFile)
					_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, String.Format("Overwriting document {0}.", nativeFileName), False)
				Else
					_parent.WriteWarning(String.Format("{0} already exists. Skipping file export.", tempFile))
					artifact.NativeTempLocation = tempFile
					Return kCura.Utility.File.Instance.Length(tempFile)
				End If
			End If
			Dim tries As Int32 = 0
			Dim maxTries As Int32 = NumberOfRetries + 1
			While tries < maxTries AndAlso Not Me.Halt
				tries += 1
				Try
					If Me.Settings.ArtifactTypeID = ArtifactType.Document Then
						_downloadManager.DownloadFileForDocument(tempFile, artifact.NativeFileGuid, artifact.NativeSourceLocation, artifact.ArtifactID, _settings.CaseArtifactID.ToString)
					Else
						_downloadManager.DownloadFileForDynamicObject(tempFile, artifact.NativeSourceLocation, artifact.ArtifactID, _settings.CaseArtifactID.ToString, artifact.FileID, Me.Settings.FileField.FieldID)
					End If
					Exit While
				Catch ex As System.Exception
					If tries = 1 Then
						_parent.WriteStatusLine(Windows.Process.EventType.Warning, "Second attempt to download native for document " & artifact.IdentifierValue, True)
					ElseIf tries < maxTries Then
						Dim waitTime As Int32 = WaitTimeBetweenRetryAttempts
						_parent.WriteStatusLine(Windows.Process.EventType.Warning, "Additional attempt to download native for document " & artifact.IdentifierValue & " failed - retrying in " & waitTime.ToString() & " seconds", True)
						System.Threading.Thread.CurrentThread.Join(waitTime * 1000)
					Else
						Throw
					End If
				End Try
			End While
			artifact.NativeTempLocation = tempFile
			_statistics.FileTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1)
			Return kCura.Utility.File.Instance.Length(tempFile)
		End Function

		Private Sub WriteLongText(ByVal source As System.IO.TextReader, ByVal output As System.IO.TextWriter, ByVal formatter As ILongTextStreamFormatter)
			Dim c As Int32 = source.Read
			Dim doTransform As Boolean = (output Is _nativeFileWriter)
			Try
				While c <> -1
					formatter.TransformAndWriteCharacter(c, output)
					c = source.Read
				End While
			Finally
				If Not source Is Nothing Then
					Try
						source.Close()
					Catch
					End Try
				End If
				If Not output Is Nothing AndAlso Not doTransform Then
					Try
						output.Close()
					Catch
					End Try
				End If
			End Try
		End Sub

		Private Function ManageLongText(ByVal sourceValue As Object, ByVal textField As Types.ViewFieldInfo, ByRef downloadedTextFilePath As String, ByVal artifact As ObjectExportInfo, ByVal startBound As String, ByVal endBound As String) As Long
			_nativeFileWriter.Write(startBound)
			If TypeOf sourceValue Is Byte() Then
				sourceValue = System.Text.Encoding.Unicode.GetString(DirectCast(sourceValue, Byte()))
			End If
			If sourceValue Is Nothing Then sourceValue = String.Empty
			Dim textValue As String = sourceValue.ToString
			Dim source As System.IO.TextReader
			Dim destination As System.IO.TextWriter = Nothing
			Dim downloadedFileExists As Boolean = Not String.IsNullOrEmpty(downloadedTextFilePath) AndAlso System.IO.File.Exists(downloadedTextFilePath)
			If textValue = RelativityConstants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN Then
				If Me.Settings.SelectedTextFields IsNot Nothing AndAlso TypeOf textField Is CoalescedTextViewField AndAlso downloadedFileExists Then
					If Settings.SelectedTextFields.Count = 1 Then
						source = Me.GetLongTextStream(downloadedTextFilePath, textField)
					Else
						Dim precedenceField As Types.ViewFieldInfo = GetFieldForLongTextPrecedenceDownload(textField, artifact)
						source = Me.GetLongTextStream(downloadedTextFilePath, precedenceField)
					End If
				Else
					source = Me.GetLongTextStream(artifact, textField)
				End If
			Else
				source = New System.IO.StringReader(textValue)
			End If
			Dim destinationPathExists As Boolean
			Dim destinationFilePath As String = String.Empty
			Dim formatter As ILongTextStreamFormatter
			If Me.Settings.ExportFullTextAsFile AndAlso TypeOf textField Is CoalescedTextViewField Then
				destinationFilePath = Me.GetLocalTextFilePath(artifact)
				destinationPathExists = System.IO.File.Exists(destinationFilePath)
				If destinationPathExists AndAlso Not _settings.Overwrite Then
					_parent.WriteWarning(destinationFilePath & " already exists. Skipping file export.")
				Else
					If destinationPathExists Then _parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Overwriting: " & destinationFilePath, False)
					destination = New System.IO.StreamWriter(destinationFilePath, False, Me.Settings.TextFileEncoding)
				End If
				formatter = New NonTransformFormatter
			Else
				If Me.Settings.LoadFileIsHtml Then
					formatter = New HtmlFileLongTextStreamFormatter(_settings, source)
				Else
					formatter = New DelimitedFileLongTextStreamFormatter(_settings, source)
				End If
				destination = _nativeFileWriter
			End If
			If String.IsNullOrEmpty(downloadedTextFilePath) AndAlso Not source Is Nothing AndAlso TypeOf source Is System.IO.StreamReader AndAlso TypeOf DirectCast(source, System.IO.StreamReader).BaseStream Is System.IO.FileStream Then
				downloadedTextFilePath = DirectCast(DirectCast(source, System.IO.StreamReader).BaseStream, System.IO.FileStream).Name
			End If
			If Not destination Is Nothing Then
				Me.WriteLongText(source, destination, formatter)
			End If
			Dim retval As Long = 0
			If destinationFilePath <> String.Empty Then
				retval = kCura.Utility.File.Instance.GetFileSize(destinationFilePath)
				Dim textLocation As String = String.Empty
				Select Case Me.Settings.TypeOfExportedFilePath
					Case ExportFile.ExportedFilePathType.Absolute
						textLocation = destinationFilePath
					Case ExportFile.ExportedFilePathType.Relative
						textLocation = ".\" & Me.CurrentVolumeLabel & "\" & Me.CurrentFullTextSubdirectoryLabel & "\" & artifact.FullTextFileName(Me.NameTextFilesAfterIdentifier)
					Case ExportFile.ExportedFilePathType.Prefix
						textLocation = Me.Settings.FilePrefix.TrimEnd("\"c) & "\" & Me.CurrentVolumeLabel & "\" & Me.CurrentFullTextSubdirectoryLabel & "\" & artifact.FullTextFileName(Me.NameTextFilesAfterIdentifier)
				End Select
				If Settings.LoadFileIsHtml Then
					_nativeFileWriter.Write("<a href='" & textLocation & "' target='_textwindow'>" & textLocation & "</a>")
				Else
					_nativeFileWriter.Write(textLocation)
				End If
			End If
			_nativeFileWriter.Write(endBound)
			Return retval
		End Function

		Private Function GetLongTextStream(ByVal artifact As ObjectExportInfo, ByVal field As Types.ViewFieldInfo) As System.IO.TextReader
			Return New System.IO.StreamReader(Me.DownloadTextFieldAsFile(artifact, field), Me.GetLongTextFieldFileEncoding(field))
		End Function

		Private Function GetLongTextStream(ByVal filename As String, ByVal field As Types.ViewFieldInfo) As System.IO.TextReader
			Return New System.IO.StreamReader(filename, Me.GetLongTextFieldFileEncoding(field))
		End Function

		Private Function GetLongTextFieldFileEncoding(ByVal field As Types.ViewFieldInfo) As System.Text.Encoding
			If field.IsUnicodeEnabled Then Return System.Text.Encoding.Unicode
			Return System.Text.Encoding.Default
		End Function


		Public Sub UpdateLoadFile(ByVal record As Object(), ByVal hasFullText As Boolean, ByVal documentArtifactID As Int32, ByVal nativeLocation As String, ByRef fullTextTempFile As String, ByVal doc As ObjectExportInfo, ByRef extractedTextByteCount As Int64)
			If _nativeFileWriter Is Nothing Then Exit Sub
			Dim count As Int32
			Dim fieldValue As String
			Dim columnName As String
			Dim location As String = nativeLocation
			Dim rowPrefix As String = _loadFileFormatter.RowPrefix
			If Not String.IsNullOrEmpty(rowPrefix) Then _nativeFileWriter.Write(rowPrefix)
			For count = 0 To _parent.Columns.Count - 1
				Dim field As Types.ViewFieldInfo = DirectCast(_parent.Columns(count), Types.ViewFieldInfo)
				columnName = field.AvfColumnName
				Dim val As Object = record(_ordinalLookup(columnName))
				If field.FieldType = FieldTypeHelper.FieldType.Text OrElse field.FieldType = FieldTypeHelper.FieldType.OffTableText Then
					If Me.Settings.LoadFileIsHtml Then
						extractedTextByteCount += Me.ManageLongText(val, field, fullTextTempFile, doc, "<td>", "</td>")
					Else
						extractedTextByteCount += Me.ManageLongText(val, field, fullTextTempFile, doc, _settings.QuoteDelimiter, _settings.QuoteDelimiter)
					End If
				Else
					If TypeOf val Is Byte() Then val = System.Text.Encoding.Unicode.GetString(DirectCast(val, Byte()))
					If field.FieldType = FieldTypeHelper.FieldType.Date AndAlso field.Category <> FieldCategory.MultiReflected Then
						If val Is System.DBNull.Value Then
							val = String.Empty
						ElseIf TypeOf val Is System.DateTime Then
							val = DirectCast(val, System.DateTime).ToString(field.FormatString)
						End If
						'If TypeOf val Is System.datete Then

						'End If
						'If Me.Settings.LoadFileIsHtml Then
						'	Dim datetime As String = kCura.Utility.NullableTypesHelper.DBNullString(val)
						'	If datetime Is Nothing OrElse datetime = "" Then
						'		val = ""
						'	Else
						'		val = System.DateTime.Parse(datetime, System.Globalization.CultureInfo.InvariantCulture).ToString(field.FormatString)
						'	End If
						'Else
						'	val = Me.ToExportableDateString(val, field.FormatString)
						'End If
					End If
					fieldValue = kCura.Utility.NullableTypesHelper.ToEmptyStringOrValue(kCura.Utility.NullableTypesHelper.DBNullString(val))
					If field.IsMultiValueField Then
						fieldValue = Me.GetMultivalueString(fieldValue, field)
					ElseIf field.IsCodeOrMulticodeField Then
						fieldValue = Me.GetCodeValueString(fieldValue)
					End If
					_nativeFileWriter.Write(_loadFileFormatter.TransformToCell(fieldValue))
				End If
				If Not count = _parent.Columns.Count - 1 AndAlso Not Me.Settings.LoadFileIsHtml Then
					_nativeFileWriter.Write(_settings.RecordDelimiter)
				End If
			Next
			Dim imagesCell As String = _loadFileFormatter.CreateImageCell(doc)
			If Not String.IsNullOrEmpty(imagesCell) Then _nativeFileWriter.Write(imagesCell)
			If _settings.ExportNative Then
				If Me.Settings.VolumeInfo.CopyFilesFromRepository Then
					_nativeFileWriter.Write(_loadFileFormatter.CreateNativeCell(location, doc))
				Else
					_nativeFileWriter.Write(_loadFileFormatter.CreateNativeCell(doc.NativeSourceLocation, doc))
				End If
			End If
			If Not String.IsNullOrEmpty(_loadFileFormatter.RowSuffix) Then _nativeFileWriter.Write(_loadFileFormatter.RowSuffix)
			_nativeFileWriter.Write(vbNewLine)
		End Sub

		Private Function ToExportableDateString(ByVal val As Object, ByVal formatString As String) As String
			Dim datetime As String = kCura.Utility.NullableTypesHelper.DBNullString(val)
			Dim retval As String
			If datetime Is Nothing OrElse datetime.Trim = "" Then
				retval = ""
			Else
				retval = System.DateTime.Parse(datetime, System.Globalization.CultureInfo.InvariantCulture).ToString(formatString)
			End If
			Return retval
		End Function

		Private Function GetCodeValueString(ByVal input As String) As String
			input = System.Web.HttpUtility.HtmlDecode(input)
			input = input.Trim(New Char() {ChrW(11)}).Replace(ChrW(11), _settings.MultiRecordDelimiter)
			Return input
		End Function

		Private Function NameTextFilesAfterIdentifier() As Boolean
			If Me.Settings.TypeOfExport = ExportFile.ExportType.Production Then
				Return _parent.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier
			Else
				Return True
			End If
		End Function

		Private Function GetMultivalueString(ByVal input As String, ByVal field As Types.ViewFieldInfo) As String
			Dim retVal As String = input
			If input.Contains("<objects>") Then
				Dim xr As New System.Xml.XmlTextReader(New System.IO.StringReader("<objects>" & input & "</objects>"))
				Dim firstTimeThrough As Boolean = True
				Dim sb As New System.Text.StringBuilder
				While xr.Read
					If xr.Name = "object" And xr.IsStartElement Then
						xr.Read()
						If firstTimeThrough Then
							firstTimeThrough = False
						Else
							sb.Append(Me.Settings.MultiRecordDelimiter)
						End If
						Dim cleanval As String = xr.Value.Trim
						Select Case field.FieldType
							Case FieldTypeHelper.FieldType.Code, FieldTypeHelper.FieldType.MultiCode
								cleanval = Me.GetCodeValueString(cleanval)
							Case FieldTypeHelper.FieldType.Date
								cleanval = Me.ToExportableDateString(cleanval, field.FormatString)
						End Select
						'If isCodeOrMulticodeField Then cleanval = Me.GetCodeValueString(cleanval)
						sb.Append(cleanval)
					End If
				End While
				xr.Close()
				retVal = sb.ToString
			End If
			Return retVal

		End Function

		Public Sub UpdateVolume()
			_currentVolumeSize = 0
			_currentImageSubdirectorySize = 0
			_currentNativeSubdirectorySize = 0
			_currentTextSubdirectorySize = 0
			_currentSubdirectoryNumber = _settings.VolumeInfo.SubdirectoryStartNumber
			_currentVolumeNumber += 1
		End Sub

		Public Sub UpdateSubdirectory()
			_currentImageSubdirectorySize = 0
			_currentNativeSubdirectorySize = 0
			_currentTextSubdirectorySize = 0
			_currentSubdirectoryNumber += 1
		End Sub

	End Class
End Namespace