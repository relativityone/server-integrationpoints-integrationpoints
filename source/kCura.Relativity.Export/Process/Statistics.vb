Namespace kCura.Relativity.Export.Process
	Public Class Statistics
		Private _metadataBytes As Int64 = 0
		Private _metadataTime As Int64 = 0
		Private _fileBytes As Int64 = 0
		Private _fileTime As Int64 = 0
		Private _sqlTime As Int64 = 0
		Private _docCount As Int64 = 0
		Private _lastAccessed As System.DateTime
		Private _documentsCreated As Int32 = 0
		Private _documentsUpdated As Int32 = 0
		Private _filesProcessed As Int32 = 0

		Public Property BatchSize As Int32 = 0

		Public Property MetadataBytes() As Int64
			Get
				Return _metadataBytes
			End Get
			Set(ByVal value As Int64)
				_lastAccessed = System.DateTime.Now
				_metadataBytes = value
			End Set
		End Property

		Public Property MetadataTime() As Int64
			Get
				Return _metadataTime
			End Get
			Set(ByVal value As Int64)
				_lastAccessed = System.DateTime.Now
				_metadataTime = value
			End Set
		End Property

		Public Property FileBytes() As Int64
			Get
				Return _fileBytes
			End Get
			Set(ByVal value As Int64)
				_lastAccessed = System.DateTime.Now
				_fileBytes = value
			End Set
		End Property

		Public Property FileTime() As Int64
			Get
				Return _fileTime
			End Get
			Set(ByVal value As Int64)
				_lastAccessed = System.DateTime.Now
				_fileTime = value
			End Set
		End Property

		Public Property SqlTime() As Int64
			Get
				Return _sqlTime
			End Get
			Set(ByVal value As Int64)
				_lastAccessed = System.DateTime.Now
				_sqlTime = value
			End Set
		End Property

		Public Property DocCount() As Int64
			Get
				Return _docCount
			End Get
			Set(ByVal value As Int64)
				_lastAccessed = System.DateTime.Now
				_docCount = value
			End Set
		End Property

		Public ReadOnly Property LastAccessed() As System.DateTime
			Get
				Return _lastAccessed
			End Get
		End Property

		Public Function ToFileSizeSpecification(ByVal value As Double) As String
			Dim prefix As String = Nothing
			Dim k As Int32
			If value <= 0 Then
				k = 0
			Else
				k = CType(System.Math.Floor(System.Math.Log(value, 1000)), Int32)
			End If
			Select Case k
				Case 0
					prefix = ""
				Case 1
					prefix = "K"
				Case 2
					prefix = "M"
				Case 3
					prefix = "G"
				Case 4
					prefix = "T"
				Case 5
					prefix = "P"
				Case 6
					prefix = "E"
				Case 7
					prefix = "B"
				Case 8
					prefix = "Y"
			End Select
			Return (value / Math.Pow(1000, k)).ToString("N2") & " " & prefix & "B"
		End Function

		Public ReadOnly Property DocumentsCreated() As Int32
			Get
				Return _documentsCreated
			End Get
		End Property

		Public ReadOnly Property DocumentsUpdated() As Int32
			Get
				Return _documentsUpdated
			End Get
		End Property

		Public ReadOnly Property FilesProcessed() As Int32
			Get
				Return _filesProcessed
			End Get
		End Property

		'Public Sub ProcessRunResults(ByVal results As kCura.EDDS.WebAPI.BulkImportManagerBase.MassImportResults)
		'	_documentsCreated += results.ArtifactsCreated
		'	_documentsUpdated += results.ArtifactsUpdated
		'	_filesProcessed += results.FilesProcessed
		'End Sub


		Public Overridable Function ToDictionary() As IDictionary
			Dim retval As New System.Collections.Specialized.HybridDictionary
			If Not Me.FileTime = 0 Then retval.Add("Average file transfer rate", ToFileSizeSpecification(Me.FileBytes / (Me.FileTime / 10000000)) & "/sec")
			If Not Me.MetadataTime = 0 Then retval.Add("Average metadata transfer rate", ToFileSizeSpecification(Me.MetadataBytes / (Me.MetadataTime / 10000000)) & "/sec")
			If Not Me.SqlTime = 0 Then retval.Add("Average SQL process rate", (Me.DocCount / (Me.SqlTime / 10000000)).ToString("N0") & " Documents/sec")
			If Not Me.BatchSize = 0 Then retval.Add("Current batch size", (Me.BatchSize).ToString("N0"))
			Return retval
		End Function
	End Class
End Namespace
