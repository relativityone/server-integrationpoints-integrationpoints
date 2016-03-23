Imports System.Collections.Generic
Imports kCura.Relativity.Export.FileObjects
Imports kCura.Relativity.Export.Service
Imports kCura.Relativity.Export.Types
Imports Relativity

Namespace kCura.Relativity.Export.Exports
	Public Class Exporter

#Region "Members"

		Private _searchManager As Service.SearchManager
		Public ExportManager As Service.ExportManager
		Private _fieldManager As Service.FieldManager
		Private _auditManager As Service.AuditManager
		Private _exportFile As ExportFile
		Private _columns As System.Collections.ArrayList
		
		Public DocumentsExported As Int32
		Public TotalExportArtifactCount As Int32
		Private WithEvents _processController As kCura.Windows.Process.Controller
		Private WithEvents _downloadHandler As FileDownloader
		Private _halt As Boolean
		Private _volumeManager As VolumeManager
		Private _productionManager As Service.ProductionManager
		Private _exportNativesToFileNamedFrom As Types.ExportNativeWithFilenameFrom
		Private _beginBatesColumn As String = ""
		Private _timekeeper As New kCura.Utility.Timekeeper
		Private _productionArtifactIDs As Int32()
		Private _lastStatusMessageTs As Long = System.DateTime.Now.Ticks
		Private _lastDocumentsExportedCountReported As Int32 = 0
		Private _statistics As New Process.ExportStatistics
		Private _lastStatisticsSnapshot As IDictionary
		Private _start As System.DateTime
		Private _warningCount As Int32 = 0
		Private _errorCount As Int32 = 0
		Private _fileCount As Int64 = 0
		Private _productionExportProduction As kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo
		Private _productionLookup As New System.Collections.Generic.Dictionary(Of Int32, kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo)

#End Region

#Region "Accessors"

		Public Property Settings() As ExportFile
			Get
				Return _exportFile
			End Get
			Set(ByVal value As ExportFile)
				_exportFile = value
			End Set
		End Property

		Public Property Columns() As System.Collections.ArrayList
			Get
				Return _columns
			End Get
			Set(ByVal value As System.Collections.ArrayList)
				_columns = value
			End Set
		End Property

		Public Property ExportNativesToFileNamedFrom() As ExportNativeWithFilenameFrom
			Get
				Return _exportNativesToFileNamedFrom
			End Get
			Set(ByVal value As ExportNativeWithFilenameFrom)
				_exportNativesToFileNamedFrom = value
			End Set
		End Property

		Public ReadOnly Property ErrorLogFileName() As String
			Get
				If Not _volumeManager Is Nothing Then
					Return _volumeManager.ErrorLogFileName
				Else
					Return Nothing
				End If
			End Get
		End Property

		Protected Overridable ReadOnly Property NumberOfRetries() As Int32
			Get
				Return kCura.Utility.Config.ExportErrorNumberOfRetries
			End Get
		End Property

		Protected Overridable ReadOnly Property WaitTimeBetweenRetryAttempts() As Int32
			Get
				Return kCura.Utility.Config.ExportErrorWaitTimeInSeconds
			End Get
		End Property

#End Region

		Public Event ShutdownEvent()
		Public Sub Shutdown()
			RaiseEvent ShutdownEvent()
		End Sub

#Region "Constructors"

		Public Sub New(ByVal exportFile As ExportFile, ByVal processController As kCura.Windows.Process.Controller)
			_searchManager = New SearchManager(exportFile.Credential, exportFile.CookieContainer)
			_downloadHandler = New FileDownloader(exportFile.Credential, exportFile.CaseInfo.DocumentPath & "\EDDS" & exportFile.CaseInfo.ArtifactID, exportFile.CaseInfo.DownloadHandlerURL, exportFile.CookieContainer, Export.Settings.AuthenticationToken)
			FileDownloader.TotalWebTime = 0
			_productionManager = New ProductionManager(exportFile.Credential, exportFile.CookieContainer)
			_auditManager = New AuditManager(exportFile.Credential, exportFile.CookieContainer)
			_fieldManager = New FieldManager(exportFile.Credential, exportFile.CookieContainer)
			Me.ExportManager = New ExportManager(exportFile.Credential, exportFile.CookieContainer)

			_halt = False
			_processController = processController
			Me.DocumentsExported = 0
			Me.TotalExportArtifactCount = 1
			Me.Settings = exportFile
			Me.Settings.FolderPath = Me.Settings.FolderPath + "\"
			Me.ExportNativesToFileNamedFrom = exportFile.ExportNativesToFileNamedFrom
		End Sub

#End Region

		Public Function ExportSearch() As Boolean
			Try
				_start = System.DateTime.Now
				Me.Search()
			Catch ex As System.Exception
				Me.WriteFatalError(String.Format("A fatal error occurred on document #{0}", Me.DocumentsExported), ex)
				If Not _volumeManager Is Nothing Then
					_volumeManager.Close()
				End If
			End Try
			Return Me.ErrorLogFileName = ""
		End Function

		Private Function IsExtractedTextSelected() As Boolean
			For Each vfi As Types.ViewFieldInfo In Me.Settings.SelectedViewFields
				If vfi.Category = FieldCategory.FullText Then Return True
			Next
			Return False
		End Function

		Private Function ExtractedTextField() As Types.ViewFieldInfo
			For Each v As Types.ViewFieldInfo In Me.Settings.AllExportableFields
				If v.Category = FieldCategory.FullText Then Return v
			Next
			Throw New System.Exception("Full text field somehow not in all fields")
		End Function

		Private Function Search() As Boolean
			Dim tries As Int32 = 0
			Dim maxTries As Int32 = NumberOfRetries + 1

			Dim typeOfExportDisplayString As String = ""
			Dim errorOutputFilePath As String = _exportFile.FolderPath & "\" & _exportFile.LoadFilesPrefix & "_img_errors.txt"
			If System.IO.File.Exists(errorOutputFilePath) AndAlso _exportFile.Overwrite Then kCura.Utility.File.Instance.Delete(errorOutputFilePath)
			Me.WriteUpdate("Retrieving export data from the server...")
			Dim startTicks As Int64 = System.DateTime.Now.Ticks
			Dim exportInitializationArgs As kCura.EDDS.WebAPI.ExportManagerBase.InitializationResults = Nothing
			Dim columnHeaderString As String = Me.LoadColumns
			Dim allAvfIds As New System.Collections.Generic.List(Of Int32)
			For i As Int32 = 0 To _columns.Count - 1
				If Not TypeOf _columns(i) Is CoalescedTextViewField Then
					allAvfIds.Add(Me.Settings.SelectedViewFields(i).AvfId)
				End If
			Next
			Dim production As kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo = Nothing

			If Me.Settings.TypeOfExport = ExportFile.ExportType.Production Then

				tries = 0
				While tries < maxTries
					tries += 1
					Try
						production = _productionManager.Read(Me.Settings.CaseArtifactID, Me.Settings.ArtifactID)
						Exit While
					Catch ex As System.Exception
						If tries < maxTries AndAlso Not (TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("Need To Re Login") <> -1) Then
							Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " & tries & ", in " & WaitTimeBetweenRetryAttempts & " seconds...", True)
							System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
						Else
							Throw
						End If
					End Try
				End While

				_productionExportProduction = production
				With _fieldManager.Read(Me.Settings.CaseArtifactID, production.BeginBatesReflectedFieldId)
					_beginBatesColumn = SqlNameHelper.GetSqlFriendlyName(.DisplayName)
					If Not allAvfIds.Contains(.ArtifactViewFieldID) Then allAvfIds.Add(.ArtifactViewFieldID)
				End With
			End If

			If Me.Settings.ExportImages AndAlso Me.Settings.LogFileFormat = LoadFileType.FileFormat.IPRO_FullText Then
				If Not Me.IsExtractedTextSelected Then
					allAvfIds.Add(Me.ExtractedTextField.AvfId)
				End If
			End If
			tries = 0
			Select Case Me.Settings.TypeOfExport
				Case ExportFile.ExportType.ArtifactSearch
					typeOfExportDisplayString = "search"
					exportInitializationArgs = CallServerWithRetry(Function() Me.ExportManager.InitializeSearchExport(_exportFile.CaseInfo.ArtifactID, Me.Settings.ArtifactID, allAvfIds.ToArray, Me.Settings.StartAtDocumentNumber + 1), maxTries)

				Case ExportFile.ExportType.ParentSearch
					typeOfExportDisplayString = "folder"
					exportInitializationArgs = CallServerWithRetry(Function() Me.ExportManager.InitializeFolderExport(Me.Settings.CaseArtifactID, Me.Settings.ViewID, Me.Settings.ArtifactID, False, allAvfIds.ToArray, Me.Settings.StartAtDocumentNumber + 1, Me.Settings.ArtifactTypeID), maxTries)

				Case ExportFile.ExportType.AncestorSearch
					typeOfExportDisplayString = "folder and subfolder"
					exportInitializationArgs = CallServerWithRetry(Function() Me.ExportManager.InitializeFolderExport(Me.Settings.CaseArtifactID, Me.Settings.ViewID, Me.Settings.ArtifactID, True, allAvfIds.ToArray, Me.Settings.StartAtDocumentNumber + 1, Me.Settings.ArtifactTypeID), maxTries)

				Case ExportFile.ExportType.Production
					typeOfExportDisplayString = "production"
					exportInitializationArgs = CallServerWithRetry(Function() Me.ExportManager.InitializeProductionExport(_exportFile.CaseInfo.ArtifactID, Me.Settings.ArtifactID, allAvfIds.ToArray, Me.Settings.StartAtDocumentNumber + 1), maxTries)

			End Select
			Me.TotalExportArtifactCount = CType(exportInitializationArgs.RowCount, Int32)
			If Me.TotalExportArtifactCount - 1 < Me.Settings.StartAtDocumentNumber Then
				Dim msg As String = String.Format("The chosen start item number ({0}) exceeds the number of {2} items in the export ({1}).  Export halted.", Me.Settings.StartAtDocumentNumber + 1, Me.TotalExportArtifactCount, vbNewLine)
				'TODO - log in backend
				'MsgBox(msg, MsgBoxStyle.Critical, "Error")
				Me.Shutdown()
				Return False
			Else
				Me.TotalExportArtifactCount -= Me.Settings.StartAtDocumentNumber
			End If
			_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - startTicks, 1)
			RaiseEvent FileTransferModeChangeEvent(_downloadHandler.UploaderType.ToString)
			_volumeManager = New VolumeManager(Me.Settings, Me.Settings.FolderPath, Me.Settings.Overwrite, Me.TotalExportArtifactCount, Me, _downloadHandler, _timekeeper, exportInitializationArgs.ColumnNames, _statistics)
			Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Created search log file.", True)
			_volumeManager.ColumnHeaderString = columnHeaderString
			Me.WriteUpdate("Data retrieved. Beginning " & typeOfExportDisplayString & " export...")

			Dim records As Object() = Nothing
			Dim start, realStart As Int32
			Dim lastRecordCount As Int32 = -1
			While lastRecordCount <> 0
				realStart = start + Me.Settings.StartAtDocumentNumber
				_timekeeper.MarkStart("Exporter_GetDocumentBlock")
				startTicks = System.DateTime.Now.Ticks
				Dim textPrecedenceAvfIds As Int32() = Nothing
				If Not Me.Settings.SelectedTextFields Is Nothing AndAlso Me.Settings.SelectedTextFields.Count > 0 Then textPrecedenceAvfIds = Me.Settings.SelectedTextFields.Select(Of Int32)(Function(f As Types.ViewFieldInfo) f.AvfId).ToArray

				If Me.Settings.TypeOfExport = ExportFile.ExportType.Production Then
					records = CallServerWithRetry(Function() Me.ExportManager.RetrieveResultsBlockForProduction(Me.Settings.CaseInfo.ArtifactID, exportInitializationArgs.RunId, Me.Settings.ArtifactTypeID, allAvfIds.ToArray, Config.ExportBatchSize, Me.Settings.MulticodesAsNested, Me.Settings.MultiRecordDelimiter, Me.Settings.NestedValueDelimiter, textPrecedenceAvfIds, Me.Settings.ArtifactID), maxTries)
				Else
					records = CallServerWithRetry(Function() Me.ExportManager.RetrieveResultsBlock(Me.Settings.CaseInfo.ArtifactID, exportInitializationArgs.RunId, Me.Settings.ArtifactTypeID, allAvfIds.ToArray, Config.ExportBatchSize, Me.Settings.MulticodesAsNested, Me.Settings.MultiRecordDelimiter, Me.Settings.NestedValueDelimiter, textPrecedenceAvfIds), maxTries)
				End If


				If records Is Nothing Then Exit While
				If Me.Settings.TypeOfExport = ExportFile.ExportType.Production AndAlso production IsNot Nothing AndAlso production.DocumentsHaveRedactions Then
					WriteStatusLineWithoutDocCount(kCura.Windows.Process.EventType.Warning, "Please Note - Documents in this production were produced with redactions applied.  Ensure that you have exported text that was generated via OCR of the redacted documents.", True)
				End If
				lastRecordCount = records.Length
				_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - startTicks, 1)
				_timekeeper.MarkEnd("Exporter_GetDocumentBlock")
				Dim artifactIDs As New ArrayList
				Dim artifactIdOrdinal As Int32 = _volumeManager.OrdinalLookup("ArtifactID")
				If records.Length > 0 Then
					For Each artifactMetadata As Object() In records
						artifactIDs.Add(artifactMetadata(artifactIdOrdinal))
					Next
					ExportChunk(DirectCast(artifactIDs.ToArray(GetType(Int32)), Int32()), records)
					artifactIDs.Clear()
					records = Nothing
				End If
				If _halt Then Exit While
			End While

			Me.WriteStatusLine(Windows.Process.EventType.Status, FileDownloader.TotalWebTime.ToString, True)
			_timekeeper.GenerateCsvReportItemsAsRows()
			_volumeManager.Finish()
			Me.AuditRun(True)
			Return Nothing
		End Function


		Private Function CallServerWithRetry(Of T)(f As Func(Of T), ByVal maxTries As Int32) As T
			Dim tries As Integer
			Dim records As T

			tries = 0
			While tries < maxTries
				tries += 1
				Try
					records = f()
					Exit While
				Catch ex As System.Exception
					If TypeOf (ex) Is System.InvalidOperationException AndAlso ex.Message.Contains("empty response") Then
						Throw New Exception("Communication with the WebAPI server has failed, possibly because values for MaximumLongTextSizeForExportInCell and/or MaximumTextVolumeForExportChunk are too large.  Please lower them and try again.", ex)
					ElseIf tries < maxTries AndAlso Not (TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("Need To Re Login") <> -1) Then
						Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " & tries & ", in " & WaitTimeBetweenRetryAttempts & " seconds...", True)
						System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
					Else
						Throw
					End If
				End Try
			End While
			Return records
		End Function

#Region "Private Helper Functions"

		Private Sub ExportChunk(ByVal documentArtifactIDs As Int32(), ByVal records As Object())
			Dim tries As Int32 = 0
			Dim maxTries As Int32 = NumberOfRetries + 1

			Dim natives As New System.Data.DataView
			Dim images As New System.Data.DataView
			Dim productionImages As New System.Data.DataView
			Dim i As Int32 = 0
			Dim productionArtifactID As Int32 = 0
			Dim start As Int64
			If Me.Settings.TypeOfExport = ExportFile.ExportType.Production Then productionArtifactID = Settings.ArtifactID
			If Me.Settings.ExportNative Then
				start = System.DateTime.Now.Ticks
				If Me.Settings.TypeOfExport = ExportFile.ExportType.Production Then
					tries = 0
					While tries < maxTries
						tries += 1
						Try
							natives.Table = _searchManager.RetrieveNativesForProduction(Me.Settings.CaseArtifactID, productionArtifactID, kCura.Utility.Array.IntArrayToCSV(documentArtifactIDs)).Tables(0)
							Exit While
						Catch ex As System.Exception
							If tries < maxTries AndAlso Not (TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("Need To Re Login") <> -1) Then
								Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " & tries & ", in " & WaitTimeBetweenRetryAttempts & " seconds...", True)
								System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
							Else
								Throw
							End If
						End Try
					End While
				ElseIf Me.Settings.ArtifactTypeID = ArtifactType.Document Then
					tries = 0
					While tries < maxTries
						tries += 1
						Try
							natives.Table = _searchManager.RetrieveNativesForSearch(Me.Settings.CaseArtifactID, kCura.Utility.Array.IntArrayToCSV(documentArtifactIDs)).Tables(0)
							Exit While
						Catch ex As System.Exception
							If tries < maxTries AndAlso Not (TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("Need To Re Login") <> -1) Then
								Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " & tries & ", in " & WaitTimeBetweenRetryAttempts & " seconds...", True)
								System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
							Else
								Throw
							End If
						End Try
					End While
				Else
					Dim dt As System.Data.DataTable = Nothing
					tries = 0
					While tries < maxTries
						tries += 1
						Try
							dt = _searchManager.RetrieveFilesForDynamicObjects(Me.Settings.CaseArtifactID, Me.Settings.FileField.FieldID, documentArtifactIDs).Tables(0)
							Exit While
						Catch ex As System.Exception
							If tries < maxTries AndAlso Not (TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("Need To Re Login") <> -1) Then
								Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " & tries & ", in " & WaitTimeBetweenRetryAttempts & " seconds...", True)
								System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
							Else
								Throw
							End If
						End Try
					End While
					If dt Is Nothing Then
						natives = Nothing
					Else
						natives.Table = dt
					End If
				End If
				_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1)
			End If
			If Me.Settings.ExportImages Then
				_timekeeper.MarkStart("Exporter_GetImagesForDocumentBlock")
				start = System.DateTime.Now.Ticks

				tries = 0
				While tries < maxTries
					tries += 1
					Try
						images.Table = Me.RetrieveImagesForDocuments(documentArtifactIDs, Me.Settings.ImagePrecedence)
						Exit While
					Catch ex As System.Exception
						If tries < maxTries AndAlso Not (TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("Need To Re Login") <> -1) Then
							Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " & tries & ", in " & WaitTimeBetweenRetryAttempts & " seconds...", True)
							System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
						Else
							Throw
						End If
					End Try
				End While

				tries = 0
				While tries < maxTries
					tries += 1
					Try
						productionImages.Table = Me.RetrieveProductionImagesForDocuments(documentArtifactIDs, Me.Settings.ImagePrecedence)
						Exit While
					Catch ex As System.Exception
						If tries < maxTries AndAlso Not (TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("Need To Re Login") <> -1) Then
							Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " & tries & ", in " & WaitTimeBetweenRetryAttempts & " seconds...", True)
							System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
						Else
							Throw
						End If
					End Try
				End While

				_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1)
				_timekeeper.MarkEnd("Exporter_GetImagesForDocumentBlock")
			End If
			Dim beginBatesColumnIndex As Int32 = -1
			If Me.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Production AndAlso _volumeManager.OrdinalLookup.ContainsKey(_beginBatesColumn) Then
				beginBatesColumnIndex = _volumeManager.OrdinalLookup(_beginBatesColumn)
			End If
			Dim identifierColumnName As String = SqlNameHelper.GetSqlFriendlyName(Me.Settings.IdentifierColumnName)
			Dim identifierColumnIndex As Int32 = _volumeManager.OrdinalLookup(identifierColumnName)
			For i = 0 To documentArtifactIDs.Length - 1
				Dim artifact As New ObjectExportInfo
				Dim record As Object() = DirectCast(records(i), Object())
				Dim nativeRow As System.Data.DataRowView = GetNativeRow(natives, documentArtifactIDs(i))
				If Me.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Production AndAlso beginBatesColumnIndex <> -1 Then
					artifact.ProductionBeginBates = record(beginBatesColumnIndex).ToString
				End If
				artifact.IdentifierValue = record(identifierColumnIndex).ToString
				artifact.Images = Me.PrepareImages(images, productionImages, documentArtifactIDs(i), artifact.IdentifierValue, artifact, Me.Settings.ImagePrecedence)
				If nativeRow Is Nothing Then
					artifact.NativeFileGuid = ""
					artifact.OriginalFileName = ""
					artifact.NativeSourceLocation = ""
				Else
					artifact.OriginalFileName = nativeRow("Filename").ToString
					artifact.NativeSourceLocation = nativeRow("Location").ToString
					If Me.Settings.ArtifactTypeID = ArtifactType.Document Then
						artifact.NativeFileGuid = nativeRow("Guid").ToString
					Else
						artifact.FileID = CType(nativeRow("FileID"), Int32)
					End If
				End If
				If nativeRow Is Nothing Then
					artifact.NativeExtension = ""
				ElseIf nativeRow("Filename").ToString.IndexOf(".") <> -1 Then
					artifact.NativeExtension = nativeRow("Filename").ToString.Substring(nativeRow("Filename").ToString.LastIndexOf(".") + 1)
				Else
					artifact.NativeExtension = ""
				End If
				artifact.ArtifactID = documentArtifactIDs(i)
				artifact.Metadata = DirectCast(records(i), Object())

				tries = 0
				While tries < maxTries
					tries += 1
					Try
						_fileCount += _volumeManager.ExportArtifact(artifact)
						Exit While
					Catch ex As System.Exception
						If tries < maxTries AndAlso Not (TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("Need To Re Login") <> -1) Then
							Me.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " & tries & ", in " & WaitTimeBetweenRetryAttempts & " seconds...", True)
							System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000)
						Else
							Throw
						End If
					End Try
				End While

				_lastStatisticsSnapshot = _statistics.ToDictionary
				Me.WriteUpdate("Exported document " & i + 1, i = documentArtifactIDs.Length - 1)
				If _halt Then Exit Sub
			Next
		End Sub

		Private Function PrepareImagesForProduction(ByVal imagesView As System.Data.DataView, ByVal documentArtifactID As Int32, ByVal batesBase As String, ByVal artifact As ObjectExportInfo) As System.Collections.ArrayList
			Dim retval As New System.Collections.ArrayList
			If Not Me.Settings.ExportImages Then Return retval
			Dim matchingRows As DataRow() = imagesView.Table.Select("DocumentArtifactID = " & documentArtifactID.ToString)
			Dim i As Int32 = 0
			'DAS034 There is at least one case where all production images for a document will end up with the same filename.
			'This happens when the production uses Existing production numbering, and the base production used Document numbering.
			'This case cannot be detected using current available information about the Production that we get from WebAPI.
			'To be on the safe side, keep track of the first image filename, and if another image has the same filename, add i + 1 onto it.
			Dim firstImageFileName As String = Nothing
			If matchingRows.Count > 0 Then
				Dim dr As System.Data.DataRow
				For Each dr In matchingRows
					Dim image As New ImageExportInfo
					image.FileName = dr("ImageFileName").ToString
					image.FileGuid = dr("ImageGuid").ToString
					image.ArtifactID = documentArtifactID
					image.PageOffset = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable(Of Int32)(dr("ByteRange"))
					image.BatesNumber = dr("BatesNumber").ToString
					image.SourceLocation = dr("Location").ToString
					Dim filenameExtension As String = ""
					If image.FileName.IndexOf(".") <> -1 Then
						filenameExtension = "." & image.FileName.Substring(image.FileName.LastIndexOf(".") + 1)
					End If
					Dim filename As String = image.BatesNumber
					If i = 0 Then
						firstImageFileName = filename
					End If
					If (i > 0) AndAlso
						(IsDocNumberOnlyProduction(_productionExportProduction) OrElse
						 filename.Equals(firstImageFileName, StringComparison.OrdinalIgnoreCase)) Then
						filename &= "_" & (i + 1).ToString
					End If
					image.FileName = filename & filenameExtension
					If Not image.FileGuid = "" Then
						retval.Add(image)
					End If
					i += 1
				Next
			End If
			Return retval
		End Function

		Private Function GetProduction(ByVal productionArtifactId As String) As kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo
			Dim id As Int32 = CInt(productionArtifactId)
			If Not _productionLookup.ContainsKey(id) Then
				_productionLookup.Add(id, _productionManager.Read(Me.Settings.CaseArtifactID, id))
			End If
			Return _productionLookup(id)
		End Function

		Private Function IsDocNumberOnlyProduction(ByVal production As kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo) As Boolean
			Return Not production Is Nothing AndAlso production.BatesNumbering = False AndAlso production.UseDocumentLevelNumbering AndAlso Not production.IncludeImageLevelNumberingForDocumentLevelNumbering
		End Function

		Private Function PrepareImages(ByVal imagesView As System.Data.DataView, ByVal productionImagesView As System.Data.DataView, ByVal documentArtifactID As Int32, ByVal batesBase As String, ByVal artifact As ObjectExportInfo, ByVal productionOrderList As Pair()) As System.Collections.ArrayList
			Dim retval As New System.Collections.ArrayList
			If Not Me.Settings.ExportImages Then Return retval
			If Me.Settings.TypeOfExport = ExportFile.ExportType.Production Then
				productionImagesView.Sort = "DocumentArtifactID ASC"
				Return Me.PrepareImagesForProduction(productionImagesView, documentArtifactID, batesBase, artifact)
			End If
			Dim item As Pair
			For Each item In productionOrderList
				If item.Value = "-1" Then
					Return Me.PrepareOriginalImages(imagesView, documentArtifactID, batesBase, artifact)
				Else
					productionImagesView.RowFilter = String.Format("DocumentArtifactID = {0} AND ProductionArtifactID = {1}", documentArtifactID, item.Value)
					If productionImagesView.Count > 0 Then
						Dim drv As System.Data.DataRowView
						Dim i As Int32 = 0
						For Each drv In productionImagesView
							Dim image As New ImageExportInfo
							image.FileName = drv("ImageFileName").ToString
							image.FileGuid = drv("ImageGuid").ToString
							If image.FileGuid <> "" Then
								image.ArtifactID = documentArtifactID
								image.BatesNumber = drv("BatesNumber").ToString
								image.PageOffset = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable(Of Int32)(drv("ByteRange"))
								Dim filenameExtension As String = ""
								If image.FileName.IndexOf(".") <> -1 Then
									filenameExtension = "." & image.FileName.Substring(image.FileName.LastIndexOf(".") + 1)
								End If
								Dim filename As String = image.BatesNumber
								If IsDocNumberOnlyProduction(Me.GetProduction(item.Value)) AndAlso i > 0 Then filename &= "_" & (i + 1).ToString
								image.FileName = filename & filenameExtension
								image.SourceLocation = drv("Location").ToString
								retval.Add(image)
								i += 1
							End If
						Next
						Return retval
					End If
				End If
			Next
			Return retval
		End Function

		Private Function PrepareOriginalImages(ByVal imagesView As System.Data.DataView, ByVal documentArtifactID As Int32, ByVal batesBase As String, ByVal artifact As ObjectExportInfo) As System.Collections.ArrayList
			Dim retval As New System.Collections.ArrayList
			If Not Me.Settings.ExportImages Then Return retval
			imagesView.RowFilter = "DocumentArtifactID = " & documentArtifactID.ToString
			Dim i As Int32 = 0
			If imagesView.Count > 0 Then
				Dim drv As System.Data.DataRowView
				For Each drv In imagesView
					Dim image As New ImageExportInfo
					image.FileName = drv("Filename").ToString
					image.FileGuid = drv("Guid").ToString
					image.ArtifactID = documentArtifactID
					image.PageOffset = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable(Of Int32)(drv("ByteRange"))
					If i = 0 Then
						image.BatesNumber = artifact.IdentifierValue
					Else
						image.BatesNumber = drv("Identifier").ToString
						If image.BatesNumber.IndexOf(image.FileGuid) <> -1 Then
							image.BatesNumber = artifact.IdentifierValue & "_" & i.ToString.PadLeft(imagesView.Count.ToString.Length + 1, "0"c)
						End If
					End If
					'image.BatesNumber = drv("Identifier").ToString
					Dim filenameExtension As String = ""
					If image.FileName.IndexOf(".") <> -1 Then
						filenameExtension = "." & image.FileName.Substring(image.FileName.LastIndexOf(".") + 1)
					End If
					image.FileName = kCura.Utility.File.Instance.ConvertIllegalCharactersInFilename(image.BatesNumber.ToString & filenameExtension)
					image.SourceLocation = drv("Location").ToString
					retval.Add(image)
					i += 1
				Next
			End If
			Return retval
		End Function

		Private Function GetNativeRow(ByVal dv As System.Data.DataView, ByVal artifactID As Int32) As System.Data.DataRowView
			If Not Me.Settings.ExportNative Then Return Nothing
			If Me.Settings.ArtifactTypeID = 10 Then
				dv.RowFilter = "DocumentArtifactID = " & artifactID.ToString
			Else
				dv.RowFilter = "ObjectArtifactID = " & artifactID.ToString
			End If
			If dv.Count > 0 Then
				Return dv(0)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>
		''' Sets the member variable _columns to contain an array of each Field which will be exported.
		''' _columns is an array of ViewFieldInfo, but for the "Text Precedence" column, the array item is
		''' a CoalescedTextViewField (a subclass of ViewFieldInfo).
		''' </summary>
		''' <returns>A string containing the contents of the export file header.  For example, if _exportFile.LoadFile is false,
		''' and the fields selected to export are (Control Number and ArtifactID), along with the Text Precendence which includes
		''' Extracted Text, then the following string would be returned: ""Control Number","Artifact ID","Text Precedence" "
		''' </returns>
		''' <remarks></remarks>
		Private Function LoadColumns() As String
			Dim retString As New System.Text.StringBuilder
			If _exportFile.LoadFileIsHtml Then
				retString.Append("<html><head><title>" & System.Web.HttpUtility.HtmlEncode(_exportFile.CaseInfo.Name) & "</title>")
				retString.Append("<style type='text/css'>" & vbNewLine)
				retString.Append("td {vertical-align: top;background-color:#EEEEEE;}" & vbNewLine)
				retString.Append("th {color:#DDDDDD;text-align:left;}" & vbNewLine)
				retString.Append("table {background-color:#000000;}" & vbNewLine)
				retString.Append("</style>" & vbNewLine)
				retString.Append("</head><body>" & vbNewLine)
				retString.Append("<table width='100%'><tr>" & vbNewLine)
			End If
			For Each field As Types.ViewFieldInfo In Me.Settings.SelectedViewFields
				Me.Settings.ExportFullText = Me.Settings.ExportFullText OrElse field.Category = FieldCategory.FullText
			Next
			_columns = New System.Collections.ArrayList(Me.Settings.SelectedViewFields)
			If Not Me.Settings.SelectedTextFields Is Nothing AndAlso Me.Settings.SelectedTextFields.Count > 0 Then
				Dim longTextSelectedViewFields As New List(Of Types.ViewFieldInfo)()
				longTextSelectedViewFields.AddRange(Me.Settings.SelectedViewFields.Where(Function(f As Types.ViewFieldInfo) f.FieldType = FieldTypeHelper.FieldType.Text OrElse f.FieldType = FieldTypeHelper.FieldType.OffTableText))
				If (Me.Settings.SelectedTextFields.Count = 1) AndAlso longTextSelectedViewFields.Exists(Function(f As Types.ViewFieldInfo) f.Equals(Me.Settings.SelectedTextFields.First)) Then
					Dim selectedViewFieldToRemove As Types.ViewFieldInfo = longTextSelectedViewFields.Find(Function(f As Types.ViewFieldInfo) f.Equals(Me.Settings.SelectedTextFields.First))
					If selectedViewFieldToRemove IsNot Nothing Then
						Dim indexOfSelectedViewFieldToRemove As Int32 = _columns.IndexOf(selectedViewFieldToRemove)
						_columns.RemoveAt(indexOfSelectedViewFieldToRemove)
						_columns.Insert(indexOfSelectedViewFieldToRemove, New CoalescedTextViewField(Me.Settings.SelectedTextFields.First, True))
					Else
						_columns.Add(New CoalescedTextViewField(Me.Settings.SelectedTextFields.First, False))
					End If
				Else
					_columns.Add(New CoalescedTextViewField(Me.Settings.SelectedTextFields.First, False))
				End If
			End If
			For i As Int32 = 0 To _columns.Count - 1
				Dim field As Types.ViewFieldInfo = DirectCast(_columns(i), Types.ViewFieldInfo)
				If _exportFile.LoadFileIsHtml Then
					retString.AppendFormat("{0}{1}{2}", "<th>", System.Web.HttpUtility.HtmlEncode(field.DisplayName), "</th>")
				Else
					retString.AppendFormat("{0}{1}{0}", Me.Settings.QuoteDelimiter, field.DisplayName)
					If i < _columns.Count - 1 Then retString.Append(Me.Settings.RecordDelimiter)
				End If
			Next

			If Not Me.Settings.LoadFileIsHtml Then retString = New System.Text.StringBuilder(retString.ToString.TrimEnd(Me.Settings.RecordDelimiter))
			If _exportFile.LoadFileIsHtml Then
				If Me.Settings.ExportImages AndAlso Me.Settings.ArtifactTypeID = ArtifactType.Document Then retString.Append("<th>Image Files</th>")
				If Me.Settings.ExportNative Then retString.Append("<th>Native Files</th>")
				retString.Append(vbNewLine & "</tr>" & vbNewLine)
			Else
				If Me.Settings.ExportNative Then retString.AppendFormat("{2}{0}{1}{0}", Me.Settings.QuoteDelimiter, "FILE_PATH", Me.Settings.RecordDelimiter)
			End If
			retString.Append(System.Environment.NewLine)
			Return retString.ToString
		End Function

		Private Function RetrieveImagesForDocuments(ByVal documentArtifactIDs As Int32(), ByVal productionOrderList As Pair()) As System.Data.DataTable
			Select Case Me.Settings.TypeOfExport
				Case ExportFile.ExportType.Production
					Return Nothing
				Case Else
					Return _searchManager.RetrieveImagesForDocuments(Me.Settings.CaseArtifactID, documentArtifactIDs).Tables(0)
			End Select
		End Function

		Private Function RetrieveProductionImagesForDocuments(ByVal documentArtifactIDs As Int32(), ByVal productionOrderList As Pair()) As System.Data.DataTable
			Select Case Me.Settings.TypeOfExport
				Case ExportFile.ExportType.Production
					Return _searchManager.RetrieveImagesForProductionDocuments(Me.Settings.CaseArtifactID, documentArtifactIDs, Int32.Parse(productionOrderList(0).Value)).Tables(0)
				Case Else
					Dim productionIDs As Int32() = Me.GetProductionArtifactIDs(productionOrderList)
					If productionIDs.Length > 0 Then Return _searchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport(Me.Settings.CaseArtifactID, productionIDs, documentArtifactIDs).Tables(0)
			End Select
			Return Nothing
		End Function

		Private Function GetProductionArtifactIDs(ByVal productionOrderList As Pair()) As Int32()
			If _productionArtifactIDs Is Nothing Then
				Dim retval As New System.Collections.ArrayList
				Dim item As Pair
				For Each item In productionOrderList
					If item.Value <> "-1" Then
						retval.Add(Int32.Parse(item.Value))
					End If
				Next
				_productionArtifactIDs = DirectCast(retval.ToArray(GetType(Int32)), Int32())
			End If
			Return _productionArtifactIDs
		End Function

#End Region


#Region "Messaging"

		Private Sub AuditRun(ByVal success As Boolean)
			Dim args As New kCura.EDDS.WebAPI.AuditManagerBase.ExportStatistics
			args.AppendOriginalFilenames = Me.Settings.AppendOriginalFileName
			args.Bound = Me.Settings.QuoteDelimiter
			args.ArtifactTypeID = Me.Settings.ArtifactTypeID
			Select Case Me.Settings.TypeOfExport
				Case ExportFile.ExportType.AncestorSearch
					args.DataSourceArtifactID = Me.Settings.ViewID
				Case ExportFile.ExportType.ArtifactSearch
					args.DataSourceArtifactID = Me.Settings.ArtifactID
				Case ExportFile.ExportType.ParentSearch
					args.DataSourceArtifactID = Me.Settings.ViewID
				Case ExportFile.ExportType.Production
					args.DataSourceArtifactID = Me.Settings.ArtifactID
			End Select
			args.Delimiter = Me.Settings.RecordDelimiter
			args.DestinationFilesystemFolder = Me.Settings.FolderPath
			args.DocumentExportCount = Me.DocumentsExported
			args.ErrorCount = _errorCount
			If Not Me.Settings.SelectedTextFields Is Nothing Then args.ExportedTextFieldID = Me.Settings.SelectedTextFields(0).FieldArtifactId
			If Me.Settings.ExportFullTextAsFile Then
				args.ExportedTextFileEncodingCodePage = Me.Settings.TextFileEncoding.CodePage
				args.ExportTextFieldAsFiles = True
			Else
				args.ExportTextFieldAsFiles = False
			End If
			Dim fields As New System.Collections.ArrayList
			For Each field As Types.ViewFieldInfo In Me.Settings.SelectedViewFields
				If Not fields.Contains(field.FieldArtifactId) Then fields.Add(field.FieldArtifactId)
			Next
			args.Fields = DirectCast(fields.ToArray(GetType(Int32)), Int32())
			args.ExportNativeFiles = Me.Settings.ExportNative
			If args.Fields.Length > 0 OrElse Me.Settings.ExportNative Then
				args.MetadataLoadFileEncodingCodePage = Me.Settings.LoadFileEncoding.CodePage
				Select Case Me.Settings.LoadFileExtension.ToLower
					Case "txt"
						args.MetadataLoadFileFormat = EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Custom
					Case "csv"
						args.MetadataLoadFileFormat = EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Csv
					Case "dat"
						args.MetadataLoadFileFormat = EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Dat
					Case "html"
						args.MetadataLoadFileFormat = EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Html
				End Select
				args.MultiValueDelimiter = Me.Settings.MultiRecordDelimiter
				args.ExportMultipleChoiceFieldsAsNested = Me.Settings.MulticodesAsNested
				args.NestedValueDelimiter = Me.Settings.NestedValueDelimiter
				args.NewlineProxy = Me.Settings.NewlineDelimiter
			End If
			Try
				args.FileExportCount = CType(_fileCount, Int32)
			Catch
			End Try
			Select Case Me.Settings.TypeOfExportedFilePath
				Case ExportFile.ExportedFilePathType.Absolute
					args.FilePathSettings = "Use Absolute Paths"
				Case ExportFile.ExportedFilePathType.Prefix
					args.FilePathSettings = "Use Prefix: " & Me.Settings.FilePrefix
				Case ExportFile.ExportedFilePathType.Relative
					args.FilePathSettings = "Use Relative Paths"
			End Select
			If Me.Settings.ExportImages Then
				args.ExportImages = True
				Select Case Me.Settings.TypeOfImage
					Case ExportFile.ImageType.MultiPageTiff
						args.ImageFileType = EDDS.WebAPI.AuditManagerBase.ImageFileExportType.MultiPageTiff
					Case ExportFile.ImageType.Pdf
						args.ImageFileType = EDDS.WebAPI.AuditManagerBase.ImageFileExportType.PDF
					Case ExportFile.ImageType.SinglePage
						args.ImageFileType = EDDS.WebAPI.AuditManagerBase.ImageFileExportType.SinglePage
				End Select
				Select Case Me.Settings.LogFileFormat
					Case LoadFileType.FileFormat.IPRO
						args.ImageLoadFileFormat = EDDS.WebAPI.AuditManagerBase.ImageLoadFileFormatType.Ipro
					Case LoadFileType.FileFormat.IPRO_FullText
						args.ImageLoadFileFormat = EDDS.WebAPI.AuditManagerBase.ImageLoadFileFormatType.IproFullText
					Case LoadFileType.FileFormat.Opticon
						args.ImageLoadFileFormat = EDDS.WebAPI.AuditManagerBase.ImageLoadFileFormatType.Opticon
				End Select
				Dim hasOriginal As Boolean = False
				Dim hasProduction As Boolean = False
				For Each pair As Pair In Me.Settings.ImagePrecedence
					If pair.Value <> "-1" Then
						hasProduction = True
					Else
						hasOriginal = True
					End If
				Next
				If hasProduction AndAlso hasOriginal Then
					args.ImagesToExport = EDDS.WebAPI.AuditManagerBase.ImagesToExportType.Both
				ElseIf hasProduction Then
					args.ImagesToExport = EDDS.WebAPI.AuditManagerBase.ImagesToExportType.Produced
				Else
					args.ImagesToExport = EDDS.WebAPI.AuditManagerBase.ImagesToExportType.Original
				End If
			Else
				args.ExportImages = False
			End If
			args.OverwriteFiles = Me.Settings.Overwrite
			Dim preclist As New System.Collections.ArrayList
			For Each pair As Pair In Me.Settings.ImagePrecedence
				preclist.Add(Int32.Parse(pair.Value))
			Next
			args.ProductionPrecedence = DirectCast(preclist.ToArray(GetType(Int32)), Int32())
			args.RunTimeInMilliseconds = CType(System.Math.Min(System.DateTime.Now.Subtract(_start).TotalMilliseconds, Int32.MaxValue), Int32)
			If Me.Settings.TypeOfExport = ExportFile.ExportType.AncestorSearch OrElse Me.Settings.TypeOfExport = ExportFile.ExportType.ParentSearch Then
				args.SourceRootFolderID = Me.Settings.ArtifactID
			End If
			args.SubdirectoryImagePrefix = Me.Settings.VolumeInfo.SubdirectoryImagePrefix(False)
			args.SubdirectoryMaxFileCount = Me.Settings.VolumeInfo.SubdirectoryMaxSize
			args.SubdirectoryNativePrefix = Me.Settings.VolumeInfo.SubdirectoryNativePrefix(False)
			args.SubdirectoryStartNumber = Me.Settings.VolumeInfo.SubdirectoryStartNumber
			args.SubdirectoryTextPrefix = Me.Settings.VolumeInfo.SubdirectoryFullTextPrefix(False)
			'args.TextAndNativeFilesNamedAfterFieldID = Me.ExportNativesToFileNamedFrom
			If Me.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier Then
				For Each field As Types.ViewFieldInfo In Me.Settings.AllExportableFields
					If field.Category = FieldCategory.Identifier Then
						args.TextAndNativeFilesNamedAfterFieldID = field.FieldArtifactId
						Exit For
					End If
				Next
			Else
				For Each field As Types.ViewFieldInfo In Me.Settings.AllExportableFields
					If field.AvfColumnName.ToLower = _beginBatesColumn.ToLower Then
						args.TextAndNativeFilesNamedAfterFieldID = field.FieldArtifactId
						Exit For
					End If
				Next
			End If
			args.TotalFileBytesExported = _statistics.FileBytes
			args.TotalMetadataBytesExported = _statistics.MetadataBytes
			Select Case Me.Settings.TypeOfExport
				Case ExportFile.ExportType.AncestorSearch
					args.Type = "Folder and Subfolders"
				Case ExportFile.ExportType.ArtifactSearch
					args.Type = "Saved Search"
				Case ExportFile.ExportType.ParentSearch
					args.Type = "Folder"
				Case ExportFile.ExportType.Production
					args.Type = "Production Set"
			End Select
			args.VolumeMaxSize = Me.Settings.VolumeInfo.VolumeMaxSize
			args.VolumePrefix = Me.Settings.VolumeInfo.VolumePrefix
			args.VolumeStartNumber = Me.Settings.VolumeInfo.VolumeStartNumber
			args.StartExportAtDocumentNumber = Me.Settings.StartAtDocumentNumber + 1
			args.CopyFilesFromRepository = Me.Settings.VolumeInfo.CopyFilesFromRepository
			args.WarningCount = _warningCount
			Try
				_auditManager.AuditExport(Me.Settings.CaseInfo.ArtifactID, Not success, args)
			Catch
			End Try
		End Sub

		Friend Sub WriteFatalError(ByVal line As String, ByVal ex As System.Exception)
			Me.AuditRun(False)
			RaiseEvent FatalErrorEvent(line, ex)
		End Sub

		Friend Sub WriteStatusLine(ByVal e As kCura.Windows.Process.EventType, ByVal line As String, ByVal isEssential As Boolean)
			Dim now As Long = System.DateTime.Now.Ticks
			If now - _lastStatusMessageTs > 10000000 OrElse isEssential Then
				_lastStatusMessageTs = now
				Dim appendString As String = " ... " & Me.DocumentsExported - _lastDocumentsExportedCountReported & " document(s) exported."
				_lastDocumentsExportedCountReported = Me.DocumentsExported
				RaiseEvent StatusMessage(New ExportEventArgs(Me.DocumentsExported, Me.TotalExportArtifactCount, line & appendString, e, _lastStatisticsSnapshot))
			End If
		End Sub

		Friend Sub WriteStatusLineWithoutDocCount(ByVal e As kCura.Windows.Process.EventType, ByVal line As String, ByVal isEssential As Boolean)
			Dim now As Long = System.DateTime.Now.Ticks
			If now - _lastStatusMessageTs > 10000000 OrElse isEssential Then
				_lastStatusMessageTs = now
				_lastDocumentsExportedCountReported = Me.DocumentsExported
				RaiseEvent StatusMessage(New ExportEventArgs(Me.DocumentsExported, Me.TotalExportArtifactCount, line, e, _lastStatisticsSnapshot))
			End If
		End Sub

		Friend Sub WriteError(ByVal line As String)
			_errorCount += 1
			WriteStatusLine(kCura.Windows.Process.EventType.Error, line, True)
		End Sub

		Friend Sub WriteImgProgressError(ByVal artifact As ObjectExportInfo, ByVal imageIndex As Int32, ByVal ex As System.Exception, Optional ByVal notes As String = "")
			Dim sw As New System.IO.StreamWriter(_exportFile.FolderPath & "\" & _exportFile.LoadFilesPrefix & "_img_errors.txt", True, _exportFile.LoadFileEncoding)
			sw.WriteLine(System.DateTime.Now.ToString("s"))
			sw.WriteLine(String.Format("DOCUMENT: {0}", artifact.IdentifierValue))
			If imageIndex > -1 AndAlso artifact.Images.Count > 0 Then
				sw.WriteLine(String.Format("IMAGE: {0} ({1} of {2})", artifact.Images(imageIndex), imageIndex + 1, artifact.Images.Count))
			End If
			If Not notes = "" Then sw.WriteLine("NOTES: " & notes)
			sw.WriteLine("ERROR: " & ex.ToString)
			sw.WriteLine("")
			sw.Flush()
			sw.Close()
			Dim errorLine As String = String.Format("Error processing images for document {0}: {1}. Check {2}_img_errors.txt for details", artifact.IdentifierValue, ex.Message.TrimEnd("."c), _exportFile.LoadFilesPrefix)
			Me.WriteError(errorLine)
		End Sub

		Friend Sub WriteWarning(ByVal line As String)
			_warningCount += 1
			WriteStatusLine(kCura.Windows.Process.EventType.Warning, line, True)
		End Sub

		Friend Sub WriteUpdate(ByVal line As String, Optional ByVal isEssential As Boolean = True)
			WriteStatusLine(kCura.Windows.Process.EventType.Progress, line, isEssential)
		End Sub

#End Region

#Region "Public Events"

		Public Event FatalErrorEvent(ByVal message As String, ByVal ex As System.Exception)
		Public Event StatusMessage(ByVal exportArgs As ExportEventArgs)
		Public Event FileTransferModeChangeEvent(ByVal mode As String)
		Public Event DisableCloseButton()
		Public Event EnableCloseButton()

#End Region

		Private Sub _processController_HaltProcessEvent(ByVal processID As System.Guid) Handles _processController.HaltProcessEvent
			_halt = True
			If Not _volumeManager Is Nothing Then _volumeManager.Halt = True
		End Sub

		Public Event UploadModeChangeEvent(ByVal mode As String)

		Private Sub _downloadHandler_UploadModeChangeEvent(ByVal mode As String) Handles _downloadHandler.UploadModeChangeEvent
			RaiseEvent FileTransferModeChangeEvent(_downloadHandler.UploaderType.ToString)
		End Sub
	End Class
End Namespace