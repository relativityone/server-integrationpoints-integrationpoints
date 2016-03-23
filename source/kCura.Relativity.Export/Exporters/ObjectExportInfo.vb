Namespace kCura.Relativity.Export.Exports
	Public Class ObjectExportInfo
		Private _images As System.Collections.ArrayList
		Private _native As Object
		Private _totalFileSize As Int64
		Private _totalNumberOfFiles As Int64
		Private _artifactID As Int32
		Private _hasFullText As Boolean
		Private _identifierValue As String = ""
		Private _nativeExtension As String = ""
		Private _nativeFileGuid As String = ""
		Private _nativeTempLocation As String = ""
		Private _productionBeginBates As String = ""
		Private _originalFileName As String = ""
		Private _nativeSourceLocation As String = ""
		Private _hasCountedNative As Boolean = False
		Private _hasCountedTextFile As Boolean = False
		Private _docCount As Int32 = 1
		Private _fileID As Int32 = 0
		Private _metadata As Object()

		Public Property Metadata() As Object()
			Get
				Return _metadata
			End Get
			Set(ByVal value As Object())
				_metadata = value
			End Set
		End Property

		Public Property Images() As System.Collections.ArrayList
			Get
				Return _images
			End Get
			Set(ByVal value As System.Collections.ArrayList)
				_images = value
			End Set
		End Property

		Public Property Native() As Object
			Get
				Return _native
			End Get
			Set(ByVal value As Object)
				_native = value
			End Set
		End Property

		Public Property HasFullText() As Boolean
			Get
				Return _hasFullText
			End Get
			Set(ByVal value As Boolean)
				_hasFullText = value
			End Set
		End Property

		Public Property TotalFileSize() As Int64
			Get
				Return _totalFileSize
			End Get
			Set(ByVal value As Int64)
				_totalFileSize = value
			End Set
		End Property

		Public Property TotalNumberOfFiles() As Int64
			Get
				Return _totalNumberOfFiles
			End Get
			Set(ByVal value As Int64)
				_totalNumberOfFiles = value
			End Set
		End Property

		Public Property IdentifierValue() As String
			Get
				Return _identifierValue
			End Get
			Set(ByVal value As String)
				_identifierValue = value
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

		Public Property NativeExtension() As String
			Get
				Return _nativeExtension
			End Get
			Set(ByVal value As String)
				_nativeExtension = value
			End Set
		End Property

		Public Property NativeFileGuid() As String
			Get
				Return _nativeFileGuid
			End Get
			Set(ByVal value As String)
				_nativeFileGuid = value
			End Set
		End Property

		Public Property NativeTempLocation() As String
			Get
				Return _nativeTempLocation
			End Get
			Set(ByVal value As String)
				_nativeTempLocation = value
			End Set
		End Property

		Public Property NativeSourceLocation() As String
			Get
				Return _nativeSourceLocation
			End Get
			Set(ByVal value As String)
				_nativeSourceLocation = value
			End Set
		End Property

		Public Function NativeFileName(ByVal appendToOriginal As Boolean) As String
			Dim retval As String
			If appendToOriginal Then
				retval = IdentifierValue & "_" & OriginalFileName
			Else
				If Not NativeExtension = "" Then
					retval = IdentifierValue & "." & NativeExtension
				Else
					retval = IdentifierValue
				End If
			End If
			Return kCura.Utility.File.Instance.ConvertIllegalCharactersInFilename(retval)
		End Function

		Public Function FullTextFileName(ByVal nameFilesAfterIdentifier As Boolean) As String
			Dim retval As String
			If Not nameFilesAfterIdentifier Then
				retval = Me.ProductionBeginBates
			Else
				retval = Me.IdentifierValue
			End If
			Return kCura.Utility.File.Instance.ConvertIllegalCharactersInFilename(retval & ".txt")
		End Function

		Public Property OriginalFileName() As String
			Get
				Return _originalFileName
			End Get
			Set(ByVal value As String)
				_originalFileName = value
			End Set
		End Property

		Public ReadOnly Property ProductionBeginBatesFileName(ByVal appendToOriginal As Boolean) As String
			Get
				Dim retval As String
				If appendToOriginal Then
					retval = ProductionBeginBates & "_" & OriginalFileName
				Else
					If Not NativeExtension = "" Then
						retval = ProductionBeginBates & "." & NativeExtension
					Else
						retval = ProductionBeginBates
					End If
				End If
				Return kCura.Utility.File.Instance.ConvertIllegalCharactersInFilename(retval)
			End Get
		End Property

		Public ReadOnly Property NativeCount() As Int64
			Get
				If Me.NativeFileGuid = "" Then
					If Not Me.FileID = Nothing Or Me.FileID <> 0 Then
						Return 1
					Else
						Return 0
					End If
				Else
					Return 1
				End If
			End Get
		End Property

		Public ReadOnly Property ImageCount() As Int64
			Get
				If Me.Images Is Nothing Then Return 0
				Return Me.Images.Count
			End Get
		End Property

		Public Property ProductionBeginBates() As String
			Get
				Return _productionBeginBates
			End Get
			Set(ByVal value As String)
				_productionBeginBates = value
			End Set
		End Property

		Public Property HasCountedNative() As Boolean
			Get
				Return _hasCountedNative
			End Get
			Set(ByVal value As Boolean)
				_hasCountedNative = value
			End Set
		End Property
		Public Property HasCountedTextFile() As Boolean
			Get
				Return _hasCountedTextFile
			End Get
			Set(ByVal value As Boolean)
				_hasCountedTextFile = value
			End Set
		End Property

		Public Property FileID() As Int32
			Get
				Return _fileID
			End Get
			Set(ByVal value As Int32)
				_fileID = value
			End Set
		End Property


		Public ReadOnly Property DocCount() As Int32
			Get
				Dim retval As Int32 = _docCount
				If retval = 1 Then _docCount -= 1
				Return retval
			End Get
		End Property
	End Class
End Namespace