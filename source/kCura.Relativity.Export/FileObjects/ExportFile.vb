Imports kCura.Relativity.Export.Exports
Imports kCura.Relativity.Export.Types
Imports Relativity

Namespace kCura.Relativity.Export.FileObjects
	''' <summary>
	''' Container class for all export settings
	''' </summary>
	''' <remarks>
	''' Writable properties that are not marked as ReadFromExisting are used in saving/loading these files to disk.  If one needs to add a property, and it needs to be save-able, make sure to add those settings to the serialize/deserialize methods.
	''' </remarks>
	<Serializable()> Public Class ExportFile
		Implements System.Runtime.Serialization.ISerializable

#Region " Members "

		Protected _caseInfo As CaseInfo
		Protected _dataTable As System.Data.DataTable
		Protected _typeOfExport As ExportType
		Protected _folderPath As String
		Protected _artifactID As Int32
		Protected _viewID As Int32
		Protected _overwrite As Boolean
		Protected _recordDelimiter As Char
		Protected _quoteDelimiter As Char
		Protected _newlineDelimiter As Char
		Protected _multiRecordDelimiter As Char
		Protected _nestedValueDelimiter As Char
		Protected _credential As Net.NetworkCredential
		Protected _cookieContainer As System.Net.CookieContainer
		Protected _exportFullText As Boolean
		Protected _exportFullTextAsFile As Boolean
		Protected _exportNative As Boolean
		Protected _logFileFormat As LoadFileType.FileFormat?
		Protected _renameFilesToIdentifier As Boolean
		Protected _identifierColumnName As String
		Protected _volumeInfo As VolumeInfo
		Protected _exportImages As Boolean
		Protected _loadFileExtension As String
		Protected _imagePrecedence As Pair()
		Protected _loadFilesPrefix As String
		Protected _filePrefix As String
		Protected _typeOfExportedFilePath As ExportedFilePathType
		Protected _typeOfImage As ImageType?
		Private _exportNativesToFileNamedFrom As ExportNativeWithFilenameFrom = ExportNativeWithFilenameFrom.Identifier
		Private _appendOriginalFileName As Boolean
		Private _loadFileIsHtml As Boolean = False
		Protected _artifactAvfLookup As Specialized.HybridDictionary
		Protected _allExportableFields As Types.ViewFieldInfo()
		Protected _selectedViewFields As Types.ViewFieldInfo()
		Protected _multicodesAsNested As Boolean
		Protected _selectedTextFields As Types.ViewFieldInfo()
		Protected _loadFileEncoding As System.Text.Encoding
		Protected _textFileEncoding As System.Text.Encoding
		Protected _volumeDigitPadding As Int32
		Protected _subdirectoryDigitPadding As Int32
		Protected _startAtDocument As Int32 = 0
		Private _artifactTypeID As Int32
		Private _fileField As DocumentField

#End Region

#Region "Public Properties"
		Public ReadOnly Property ArtifactTypeID() As Int32
			Get
				Return _artifactTypeID
			End Get
		End Property

		Public Property LoadFilesPrefix() As String
			Get
				Return _loadFilesPrefix
			End Get
			Set(ByVal value As String)
				_loadFilesPrefix = Utility.GetFilesystemSafeName(value)
			End Set
		End Property

		Public Property ImagePrecedence() As Pair()
			Get
				Return _imagePrecedence
			End Get
			Set(ByVal value As Pair())
				_imagePrecedence = value
			End Set
		End Property

		<ReadFromExisting()> Public Property CaseInfo() As CaseInfo
			Get
				Return _caseInfo
			End Get
			Set(ByVal value As CaseInfo)
				_caseInfo = value
			End Set
		End Property

		Public ReadOnly Property CaseArtifactID() As Int32
			Get
				Return Me.CaseInfo.ArtifactID
			End Get
		End Property

		<ReadFromExisting()> Public Property DataTable() As System.Data.DataTable
			Get
				Return _dataTable
			End Get
			Set(ByVal value As System.Data.DataTable)
				_dataTable = value
			End Set
		End Property

		<ReadFromExisting()> Public Property ArtifactAvfLookup() As Specialized.HybridDictionary
			Get
				Return _artifactAvfLookup
			End Get
			Set(ByVal value As Specialized.HybridDictionary)
				_artifactAvfLookup = value
			End Set
		End Property

		Public Property NestedValueDelimiter() As Char
			Get
				Return _nestedValueDelimiter
			End Get
			Set(ByVal value As Char)
				_nestedValueDelimiter = value
			End Set
		End Property

		<ReadFromExisting()> Public Property TypeOfExport() As ExportType
			Get
				Return _typeOfExport
			End Get
			Set(ByVal value As ExportType)
				_typeOfExport = value
			End Set
		End Property

		Public Property FolderPath() As String
			Get
				Return _folderPath
			End Get
			Set(ByVal value As String)
				_folderPath = value
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

		Public Property ViewID() As Int32
			Get
				Return _viewID
			End Get
			Set(ByVal value As Int32)
				_viewID = value
			End Set
		End Property

		Public Property Overwrite() As Boolean
			Get
				Return _overwrite
			End Get
			Set(ByVal value As Boolean)
				_overwrite = value
			End Set
		End Property

		Public Property RecordDelimiter() As Char
			Get
				Return _recordDelimiter
			End Get
			Set(ByVal value As Char)
				_recordDelimiter = value
			End Set
		End Property

		Public Property QuoteDelimiter() As Char
			Get
				Return _quoteDelimiter
			End Get
			Set(ByVal value As Char)
				_quoteDelimiter = value
			End Set
		End Property

		Public Property NewlineDelimiter() As Char
			Get
				Return _newlineDelimiter
			End Get
			Set(ByVal value As Char)
				_newlineDelimiter = value
			End Set
		End Property

		Public Property MultiRecordDelimiter() As Char
			Get
				Return _multiRecordDelimiter
			End Get
			Set(ByVal value As Char)
				_multiRecordDelimiter = value
			End Set
		End Property

		<ReadFromExisting()> Public Property Credential() As Net.NetworkCredential
			Get
				Return _credential
			End Get
			Set(ByVal value As Net.NetworkCredential)
				_credential = value
			End Set
		End Property

		<ReadFromExisting()> Public Property CookieContainer() As System.Net.CookieContainer
			Get
				Return _cookieContainer
			End Get
			Set(ByVal value As System.Net.CookieContainer)
				_cookieContainer = value
			End Set
		End Property

		Public Property ExportFullText() As Boolean
			Get
				Return _exportFullText
			End Get
			Set(ByVal value As Boolean)
				_exportFullText = value
			End Set
		End Property

		Public Property ExportFullTextAsFile() As Boolean
			Get
				Return _exportFullTextAsFile
			End Get
			Set(ByVal value As Boolean)
				_exportFullTextAsFile = value
			End Set
		End Property

		Public Property ExportNative() As Boolean
			Get
				Return _exportNative
			End Get
			Set(ByVal value As Boolean)
				_exportNative = value
			End Set
		End Property

		Public Property LogFileFormat() As LoadFileType.FileFormat?
			Get
				Return _logFileFormat
			End Get
			Set(ByVal value As LoadFileType.FileFormat?)
				_logFileFormat = value
			End Set
		End Property

		Public Property RenameFilesToIdentifier() As Boolean
			Get
				Return _renameFilesToIdentifier
			End Get
			Set(ByVal value As Boolean)
				_renameFilesToIdentifier = value
			End Set
		End Property

		Public Property IdentifierColumnName() As String
			Get
				Return _identifierColumnName
			End Get
			Set(ByVal value As String)
				_identifierColumnName = value
			End Set
		End Property

		Public Property LoadFileExtension() As String
			Get
				Return _loadFileExtension
			End Get
			Set(ByVal value As String)
				_loadFileExtension = value
			End Set
		End Property

		Public Property VolumeInfo() As VolumeInfo
			Get
				Return _volumeInfo
			End Get
			Set(ByVal value As VolumeInfo)
				_volumeInfo = value
			End Set
		End Property

		Public Property ExportImages() As Boolean
			Get
				Return _exportImages
			End Get
			Set(ByVal value As Boolean)
				_exportImages = value
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

		Public Property FilePrefix() As String
			Get
				Return _filePrefix
			End Get
			Set(ByVal value As String)
				_filePrefix = value
			End Set
		End Property

		Public Property TypeOfExportedFilePath() As ExportedFilePathType
			Get
				Return _typeOfExportedFilePath
			End Get
			Set(ByVal value As ExportedFilePathType)
				_typeOfExportedFilePath = value
			End Set
		End Property

		Public Property TypeOfImage() As ImageType?
			Get
				Return _typeOfImage
			End Get
			Set(ByVal value As ImageType?)
				_typeOfImage = value
			End Set
		End Property

		Public Property AppendOriginalFileName() As Boolean
			Get
				Return _appendOriginalFileName
			End Get
			Set(ByVal value As Boolean)
				_appendOriginalFileName = value
			End Set
		End Property

		Public Property LoadFileIsHtml() As Boolean
			Get
				Return _loadFileIsHtml
			End Get
			Set(ByVal value As Boolean)
				_loadFileIsHtml = value
			End Set
		End Property

		<ReadFromExisting()> Public Property AllExportableFields() As Types.ViewFieldInfo()
			Get
				Return _allExportableFields
			End Get
			Set(ByVal value As Types.ViewFieldInfo())
				_allExportableFields = value
			End Set
		End Property

		Public Property SelectedViewFields() As Types.ViewFieldInfo()
			Get
				Return _selectedViewFields
			End Get
			Set(ByVal value As Types.ViewFieldInfo())
				_selectedViewFields = value
			End Set
		End Property

		Public Property MulticodesAsNested() As Boolean
			Get
				Return _multicodesAsNested
			End Get
			Set(ByVal value As Boolean)
				_multicodesAsNested = value
			End Set
		End Property

		Public Property SelectedTextFields() As Types.ViewFieldInfo()
			Get
				Return _selectedTextFields
			End Get
			Set(ByVal value As Types.ViewFieldInfo())
				_selectedTextFields = value
			End Set
		End Property

		Public Property LoadFileEncoding() As System.Text.Encoding
			Get
				Return _loadFileEncoding
			End Get
			Set(ByVal value As System.Text.Encoding)
				_loadFileEncoding = value
			End Set
		End Property

		Public Property TextFileEncoding() As System.Text.Encoding
			Get
				Return _textFileEncoding
			End Get
			Set(ByVal value As System.Text.Encoding)
				_textFileEncoding = value
			End Set
		End Property

		Public Property VolumeDigitPadding() As Int32
			Get
				Return _volumeDigitPadding
			End Get
			Set(ByVal value As Int32)
				_volumeDigitPadding = value
			End Set
		End Property

		Public Property SubdirectoryDigitPadding() As Int32
			Get
				Return _subdirectoryDigitPadding
			End Get
			Set(ByVal value As Int32)
				_subdirectoryDigitPadding = value
			End Set
		End Property

		Public Property StartAtDocumentNumber() As Int32
			Get
				Return _startAtDocument
			End Get
			Set(ByVal value As Int32)
				_startAtDocument = value
			End Set
		End Property

		<ReadFromExisting()> Public Property FileField() As DocumentField
			Get
				Return _fileField
			End Get
			Set(ByVal value As DocumentField)
				_fileField = value
			End Set
		End Property

		Public ReadOnly Property HasFileField() As Boolean
			Get
				Return Not _fileField Is Nothing
			End Get
		End Property

		Public Property ObjectTypeName As String

#End Region

#Region " Serialization "

		Public Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData
			info.AddValue("ArtifactID", Me.ArtifactID, GetType(Int32))
			info.AddValue("LoadFilesPrefix", System.Web.HttpUtility.HtmlEncode(Me.LoadFilesPrefix), GetType(String))
			info.AddValue("NestedValueDelimiter", AscW(Me.NestedValueDelimiter), GetType(Int32))
			info.AddValue("TypeOfExport", CInt(Me.TypeOfExport), GetType(Int32))
			info.AddValue("FolderPath", Me.FolderPath, GetType(String))
			info.AddValue("ViewID", Me.ViewID, GetType(Int32))
			info.AddValue("Overwrite", Me.Overwrite, GetType(Boolean))
			info.AddValue("RecordDelimiter", AscW(Me.RecordDelimiter), GetType(Int32))
			info.AddValue("QuoteDelimiter", AscW(Me.QuoteDelimiter), GetType(Int32))
			info.AddValue("NewlineDelimiter", AscW(Me.NewlineDelimiter), GetType(Int32))
			info.AddValue("MultiRecordDelimiter", AscW(Me.MultiRecordDelimiter), GetType(Int32))
			info.AddValue("ExportFullText", Me.ExportFullText, GetType(Boolean))
			info.AddValue("ExportFullTextAsFile", Me.ExportFullTextAsFile, GetType(Boolean))
			info.AddValue("ExportNative", Me.ExportNative, GetType(Boolean))
			info.AddValue("LogFileFormat", If(Me.LogFileFormat.HasValue, CInt(Me.LogFileFormat.Value).ToString, String.Empty), GetType(String))
			info.AddValue("RenameFilesToIdentifier", Me.RenameFilesToIdentifier, GetType(Boolean))
			info.AddValue("IdentifierColumnName", Me.IdentifierColumnName, GetType(String))
			info.AddValue("LoadFileExtension", Me.LoadFileExtension, GetType(String))
			info.AddValue("ExportImages", Me.ExportImages, GetType(Boolean))
			info.AddValue("ExportNativesToFileNamedFrom", CInt(Me.ExportNativesToFileNamedFrom), GetType(Int32))
			info.AddValue("FilePrefix", Me.FilePrefix, GetType(String))
			info.AddValue("TypeOfExportedFilePath", CInt(Me.TypeOfExportedFilePath), GetType(Int32))
			info.AddValue("TypeOfImage", If(Me.TypeOfImage.HasValue, CInt(Me.TypeOfImage.Value).ToString, String.Empty), GetType(String))
			info.AddValue("AppendOriginalFileName", Me.AppendOriginalFileName, GetType(Boolean))
			info.AddValue("LoadFileIsHtml", Me.LoadFileIsHtml, GetType(Boolean))
			info.AddValue("MulticodesAsNested", Me.MulticodesAsNested, GetType(Boolean))
			info.AddValue("LoadFileEncoding", If(Me.LoadFileEncoding Is Nothing, -1, Me.LoadFileEncoding.CodePage), GetType(Int32))
			info.AddValue("TextFileEncoding", If(Me.TextFileEncoding Is Nothing, -1, Me.TextFileEncoding.CodePage), GetType(Int32))
			info.AddValue("VolumeDigitPadding", Me.VolumeDigitPadding, GetType(Int32))
			info.AddValue("SubdirectoryDigitPadding", Me.SubdirectoryDigitPadding, GetType(Int32))
			info.AddValue("StartAtDocumentNumber", Me.StartAtDocumentNumber, GetType(Int32))
			info.AddValue("VolumeInfo", Me.VolumeInfo, GetType(VolumeInfo))
			info.AddValue("SelectedTextFields", Me.SelectedTextFields, GetType(Types.ViewFieldInfo()))
			info.AddValue("ImagePrecedence", Me.ImagePrecedence, GetType(Pair()))
			info.AddValue("SelectedViewFields", Me.SelectedViewFields, GetType(Types.ViewFieldInfo()))
			info.AddValue("ObjectTypeName", Me.ObjectTypeName, GetType(String))
		End Sub

		Private Sub New(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)
			With info
				Me.ArtifactID = info.GetInt32("ArtifactID")
				Me.LoadFilesPrefix = System.Web.HttpUtility.HtmlDecode(info.GetString("LoadFilesPrefix"))
				Me.NestedValueDelimiter = ChrW(info.GetInt32("NestedValueDelimiter"))
				Me.TypeOfExport = CType(info.GetInt32("TypeOfExport"), ExportFile.ExportType)
				Me.FolderPath = info.GetString("FolderPath")
				Me.ViewID = info.GetInt32("ViewID")
				Me.Overwrite = info.GetBoolean("Overwrite")
				Me.RecordDelimiter = ChrW(info.GetInt32("RecordDelimiter"))
				Me.QuoteDelimiter = ChrW(info.GetInt32("QuoteDelimiter"))
				Me.NewlineDelimiter = ChrW(info.GetInt32("NewlineDelimiter"))
				Me.MultiRecordDelimiter = ChrW(info.GetInt32("MultiRecordDelimiter"))
				Me.ExportFullText = info.GetBoolean("ExportFullText")
				Me.ExportFullTextAsFile = info.GetBoolean("ExportFullTextAsFile")
				Me.ExportNative = info.GetBoolean("ExportNative")
				With kCura.Utility.NullableTypesHelper.ToNullableInt32(info.GetString("LogFileFormat"))
					Me.LogFileFormat = Nothing
					If .HasValue Then Me.LogFileFormat = CType(.Value, LoadFileType.FileFormat)
				End With
				Me.RenameFilesToIdentifier = info.GetBoolean("RenameFilesToIdentifier")
				Me.IdentifierColumnName = info.GetString("IdentifierColumnName")
				Me.LoadFileExtension = info.GetString("LoadFileExtension")
				Me.ExportImages = info.GetBoolean("ExportImages")
				Me.ExportNativesToFileNamedFrom = CType(info.GetInt32("ExportNativesToFileNamedFrom"), ExportNativeWithFilenameFrom)
				Me.FilePrefix = info.GetString("FilePrefix")
				Me.TypeOfExportedFilePath = CType(info.GetInt32("TypeOfExportedFilePath"), ExportFile.ExportedFilePathType)
				With kCura.Utility.NullableTypesHelper.ToNullableInt32(info.GetString("TypeOfImage"))
					Me.TypeOfImage = Nothing
					If .HasValue Then Me.TypeOfImage = CType(.Value, ExportFile.ImageType)
				End With
				Me.AppendOriginalFileName = info.GetBoolean("AppendOriginalFileName")
				Me.LoadFileIsHtml = info.GetBoolean("LoadFileIsHtml")
				Me.MulticodesAsNested = info.GetBoolean("MulticodesAsNested")
				Dim encod As Int32 = info.GetInt32("LoadFileEncoding")
				Me.LoadFileEncoding = If(encod > 0, System.Text.Encoding.GetEncoding(encod), Nothing)
				encod = info.GetInt32("TextFileEncoding")
				Me.TextFileEncoding = If(encod > 0, System.Text.Encoding.GetEncoding(encod), Nothing)
				Me.VolumeDigitPadding = info.GetInt32("VolumeDigitPadding")
				Me.SubdirectoryDigitPadding = info.GetInt32("SubdirectoryDigitPadding")
				Me.StartAtDocumentNumber = info.GetInt32("StartAtDocumentNumber")
				Me.VolumeInfo = CType(info.GetValue("VolumeInfo", GetType(VolumeInfo)), VolumeInfo)
				Try
					Me.SelectedTextFields = DirectCast(info.GetValue("SelectedTextFields", GetType(Types.ViewFieldInfo())), Types.ViewFieldInfo())
				Catch
					dIM a  As Object = info.GetValue("SelectedTextField", GetType(Types.ViewFieldInfo))
					Dim field As Types.ViewFieldInfo = DirectCast(a, Types.ViewFieldInfo)
					Me.SelectedTextFields = If(field Is Nothing, Nothing, {field})
				End Try
				Me.ImagePrecedence = DirectCast(info.GetValue("ImagePrecedence", GetType(Pair())), Pair())
				Me.SelectedViewFields = DirectCast(info.GetValue("SelectedViewFields", GetType(Types.ViewFieldInfo())), Types.ViewFieldInfo())
				Me.ObjectTypeName = info.GetString("ObjectTypeName")
			End With
		End Sub

#End Region

#Region " Constructors "


		Public Sub New(ByVal artifactTypeID As Int32)
			Me.RecordDelimiter = ChrW(20)
			Me.QuoteDelimiter = ChrW(254)
			Me.NewlineDelimiter = ChrW(174)
			Me.MultiRecordDelimiter = ChrW(59)
			Me.NestedValueDelimiter = "\"c
			Me.MulticodesAsNested = True
			_artifactTypeID = artifactTypeID
		End Sub

#End Region

#Region " Enums "


		Public Enum ExportType
			Production = 0
			ArtifactSearch = 1
			ParentSearch = 2
			AncestorSearch = 3
		End Enum

		Public Enum ExportedFilePathType
			Relative = 0
			Absolute = 1
			Prefix = 2
		End Enum

		Public Enum ImageType
			[Select] = -1
			SinglePage = 0
			MultiPageTiff = 1
			Pdf = 2
		End Enum
		Public Class ImageTypeParser
			Public Function Parse(ByVal s As String) As ImageType?
				If String.IsNullOrEmpty(s) Then Return Nothing
				Dim retval As New ImageType?
				Select Case s
					Case "Single-page TIF/JPG"
						retval = ImageType.SinglePage
					Case "Multi-page TIF"
						retval = ImageType.MultiPageTiff
					Case "PDF"
						retval = ImageType.Pdf
				End Select
				Return retval
			End Function
		End Class
#End Region

	End Class
End Namespace