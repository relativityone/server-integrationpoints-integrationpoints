Imports kCura.Relativity.Export.Exports
Imports kCura.Relativity.Export.FileObjects
Imports kCura.Relativity.Export.Types

Namespace kCura.Relativity.Export.Process

	Public Class ExportSearchProcess
		Inherits kCura.Windows.Process.ProcessBase

		Public ExportFile As ExportFile
		Private WithEvents _searchExporter As Exporter
		Private _startTime As System.DateTime
		Private _errorCount As Int32
		Private _warningCount As Int32
		Private _uploadModeText As String = Nothing

		Protected Overrides Sub Execute()
			_startTime = DateTime.Now
			_warningCount = 0
			_errorCount = 0
			_searchExporter = New Exporter(Me.ExportFile, Me.ProcessController)

			If Not _searchExporter.ExportSearch() Then
				Me.ProcessObserver.RaiseProcessCompleteEvent(False, _searchExporter.ErrorLogFileName)
			Else
				Me.ProcessObserver.RaiseStatusEvent("", "Export completed")
				Me.ProcessObserver.RaiseProcessCompleteEvent()
			End If
		End Sub

		Private Sub _searchExporter_FileTransferModeChangeEvent(ByVal mode As String) Handles _searchExporter.FileTransferModeChangeEvent
			If _uploadModeText Is Nothing Then
				_uploadModeText = Config.FileTransferModeExplanationText(False)
			End If
			Me.ProcessObserver.RaiseStatusBarEvent("File Transfer Mode: " & mode, _uploadModeText)
		End Sub

		Private Sub _productionExporter_StatusMessage(ByVal e As ExportEventArgs) Handles _searchExporter.StatusMessage
			Select Case e.EventType
				Case kCura.Windows.Process.EventType.Error
					_errorCount += 1
					Me.ProcessObserver.RaiseErrorEvent(e.DocumentsExported.ToString, e.Message)
				Case kCura.Windows.Process.EventType.Progress
					Me.ProcessObserver.RaiseStatusEvent("", e.Message)
				Case kCura.Windows.Process.EventType.Status
					Me.ProcessObserver.RaiseStatusEvent(e.DocumentsExported.ToString, e.Message)
				Case kCura.Windows.Process.EventType.Warning
					_warningCount += 1
					Me.ProcessObserver.RaiseWarningEvent(e.DocumentsExported.ToString, e.Message)
			End Select
			Dim statDict As IDictionary = Nothing
			If Not e.AdditionalInfo Is Nothing AndAlso TypeOf e.AdditionalInfo Is IDictionary Then
				statDict = DirectCast(e.AdditionalInfo, IDictionary)
			End If

			Me.ProcessObserver.RaiseProgressEvent(e.TotalDocuments, e.DocumentsExported, _warningCount, _errorCount, _startTime, New DateTime, Nothing, Nothing, statDict)
		End Sub

		Private Sub _productionExporter_FatalErrorEvent(ByVal message As String, ByVal ex As System.Exception) Handles _searchExporter.FatalErrorEvent
			Me.ProcessObserver.RaiseFatalExceptionEvent(ex)
		End Sub

		Private Sub _searchExporter_ShutdownEvent() Handles _searchExporter.ShutdownEvent
			Me.ProcessObserver.Shutdown()
		End Sub
	End Class
End Namespace