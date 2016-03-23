Imports kCura.Relativity.Export.FileObjects

Namespace kCura.Relativity.Export.Exports.LineFactory
	Public MustInherit Class ImageLineFactoryBase
		Inherits LineFactoryBase
		Private _batesNumber As String
		Private _pageNumber As Int32
		Private _imageType As ExportFile.ImageType
		Private _volumeName As String
		Private _fullFilePath As String


		Protected Sub New(ByVal batesNumber As String, ByVal pageNumber As Int32, ByVal fullFilePath As String, ByVal volumeName As String, ByVal imageType As ExportFile.ImageType)
			_batesNumber = batesNumber
			_pageNumber = pageNumber
			_imageType = imageType
			_volumeName = volumeName
			_fullFilePath = fullFilePath
		End Sub

		Protected ReadOnly Property BatesNumber() As String
			Get
				Return _batesNumber
			End Get
		End Property

		Protected ReadOnly Property PageNumber() As Int32
			Get
				Return _pageNumber
			End Get
		End Property

		Protected ReadOnly Property VolumeName() As String
			Get
				Return _volumeName
			End Get
		End Property

		Protected ReadOnly Property ImageType() As ExportFile.ImageType
			Get
				Return _imageType
			End Get
		End Property

		Protected ReadOnly Property FullFilePath() As String
			Get
				Return _fullFilePath
			End Get
		End Property


	End Class
End Namespace

