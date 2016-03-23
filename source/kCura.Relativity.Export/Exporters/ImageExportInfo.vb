Namespace kCura.Relativity.Export.Exports
	Public Class ImageExportInfo
		Private _fileName As String
		Private _fileGuid As String
		Private _artifactID As Int32
		Private _batesNumber As String
		Private _tempLocation As String
		Private _sourceLocation As String
		Private _pageOffset As Nullable(Of Int32)
		Private _hasBeenDownloaded As Boolean = False

		Public Property FileName() As String
			Get
				Return _fileName
			End Get
			Set(ByVal value As String)
				_fileName = value
			End Set
		End Property

		Public Property FileGuid() As String
			Get
				Return _fileGuid
			End Get
			Set(ByVal value As String)
				_fileGuid = value
			End Set
		End Property

		Public Property ArtifactID() As Int32
			Get
				Return _artifactID
			End Get
			Set(ByVal value As Int32)
				_artifactID = value
			End Set
		End Property

		Public Property BatesNumber() As String
			Get
				Return _batesNumber
			End Get
			Set(ByVal value As String)
				_batesNumber = value
			End Set
		End Property

		Public Property TempLocation() As String
			Get
				Return _tempLocation
			End Get
			Set(ByVal value As String)
				_tempLocation = value
			End Set
		End Property

		Public Property SourceLocation() As String
			Get
				Return _sourceLocation
			End Get
			Set(ByVal value As String)
				_sourceLocation = value
			End Set
		End Property

		Public Property PageOffset() As Nullable(Of Int32)
			Get
				Return _pageOffset
			End Get
			Set(ByVal value As Nullable(Of Int32))
				_pageOffset = value
			End Set
		End Property

		Public Property HasBeenCounted() As Boolean
			Get
				Return _hasBeenDownloaded
			End Get
			Set(ByVal value As Boolean)
				_hasBeenDownloaded = value
			End Set
		End Property

	End Class
End Namespace