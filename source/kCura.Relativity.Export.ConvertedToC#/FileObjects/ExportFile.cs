using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.Exports;
using kCura.Relativity.Export.Types;
using Relativity;

namespace kCura.Relativity.Export.FileObjects
{
	/// <summary>
	/// Container class for all export settings
	/// </summary>
	/// <remarks>
	/// Writable properties that are not marked as ReadFromExisting are used in saving/loading these files to disk.  If one needs to add a property, and it needs to be save-able, make sure to add those settings to the serialize/deserialize methods.
	/// </remarks>
	[Serializable()]
	public class ExportFile : System.Runtime.Serialization.ISerializable
	{

		#region " Members "

		protected CaseInfo _caseInfo;
		protected System.Data.DataTable _dataTable;
		protected ExportType _typeOfExport;
		protected string _folderPath;
		protected Int32 _artifactID;
		protected Int32 _viewID;
		protected bool _overwrite;
		protected char _recordDelimiter;
		protected char _quoteDelimiter;
		protected char _newlineDelimiter;
		protected char _multiRecordDelimiter;
		protected char _nestedValueDelimiter;
		protected System.Net.NetworkCredential _credential;
		protected System.Net.CookieContainer _cookieContainer;
		protected bool _exportFullText;
		protected bool _exportFullTextAsFile;
		protected bool _exportNative;
		protected kCura.Relativity.Export.Types.LoadFileType.FileFormat? _logFileFormat;
		protected bool _renameFilesToIdentifier;
		protected string _identifierColumnName;
		protected VolumeInfo _volumeInfo;
		protected bool _exportImages;
		protected string _loadFileExtension;
		protected IEnumerable<Pair> _imagePrecedence;
		protected string _loadFilesPrefix;
		protected string _filePrefix;
		protected ExportedFilePathType _typeOfExportedFilePath;
		protected ImageType? _typeOfImage;
		private ExportNativeWithFilenameFrom _exportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;
		private bool _appendOriginalFileName;
		private bool _loadFileIsHtml = false;
		protected System.Collections.Specialized.HybridDictionary _artifactAvfLookup;
		protected Types.ViewFieldInfo[] _allExportableFields;
		protected Types.ViewFieldInfo[] _selectedViewFields;
		protected bool _multicodesAsNested;
		protected Types.ViewFieldInfo[] _selectedTextFields;
		protected System.Text.Encoding _loadFileEncoding;
		protected System.Text.Encoding _textFileEncoding;
		protected Int32 _volumeDigitPadding;
		protected Int32 _subdirectoryDigitPadding;
		protected Int32 _startAtDocument = 0;
		private Int32 _artifactTypeID;

		private DocumentField _fileField;
		#endregion

		#region "Public Properties"
		public Int32 ArtifactTypeID {
			get { return _artifactTypeID; }
		}

		public string LoadFilesPrefix {
			get { return _loadFilesPrefix; }
			set { _loadFilesPrefix = Utility.GetFilesystemSafeName(value); }
		}

		public IEnumerable<Pair> ImagePrecedence {
			get { return _imagePrecedence; }
			set { _imagePrecedence = value; }
		}

		[ReadFromExisting()]
		public CaseInfo CaseInfo {
			get { return _caseInfo; }
			set { _caseInfo = value; }
		}

		public Int32 CaseArtifactID {
			get { return this.CaseInfo.ArtifactID; }
		}

		[ReadFromExisting()]
		public System.Data.DataTable DataTable {
			get { return _dataTable; }
			set { _dataTable = value; }
		}


		public char NestedValueDelimiter {
			get { return _nestedValueDelimiter; }
			set { _nestedValueDelimiter = value; }
		}

		[ReadFromExisting()]
		public ExportType TypeOfExport {
			get { return _typeOfExport; }
			set { _typeOfExport = value; }
		}

		public string FolderPath {
			get { return _folderPath; }
			set { _folderPath = value; }
		}

		public Int32 ArtifactID {
			get { return _artifactID; }
			set { _artifactID = value; }
		}

		public Int32 ViewID {
			get { return _viewID; }
			set { _viewID = value; }
		}

		public bool Overwrite {
			get { return _overwrite; }
			set { _overwrite = value; }
		}

		public char RecordDelimiter {
			get { return _recordDelimiter; }
			set { _recordDelimiter = value; }
		}

		public char QuoteDelimiter {
			get { return _quoteDelimiter; }
			set { _quoteDelimiter = value; }
		}

		public char NewlineDelimiter {
			get { return _newlineDelimiter; }
			set { _newlineDelimiter = value; }
		}

		public char MultiRecordDelimiter {
			get { return _multiRecordDelimiter; }
			set { _multiRecordDelimiter = value; }
		}

		[ReadFromExisting()]
		public System.Net.NetworkCredential Credential {
			get { return _credential; }
			set { _credential = value; }
		}

		[ReadFromExisting()]
		public System.Net.CookieContainer CookieContainer {
			get { return _cookieContainer; }
			set { _cookieContainer = value; }
		}

		public bool ExportFullText {
			get { return _exportFullText; }
			set { _exportFullText = value; }
		}

		public bool ExportFullTextAsFile {
			get { return _exportFullTextAsFile; }
			set { _exportFullTextAsFile = value; }
		}

		public bool ExportNative {
			get { return _exportNative; }
			set { _exportNative = value; }
		}

		public LoadFileType.FileFormat? LogFileFormat {
			get { return _logFileFormat; }
			set { _logFileFormat = value; }
		}

		public bool RenameFilesToIdentifier {
			get { return _renameFilesToIdentifier; }
			set { _renameFilesToIdentifier = value; }
		}

		public string IdentifierColumnName {
			get { return _identifierColumnName; }
			set { _identifierColumnName = value; }
		}

		public string LoadFileExtension {
			get { return _loadFileExtension; }
			set { _loadFileExtension = value; }
		}

		public VolumeInfo VolumeInfo {
			get { return _volumeInfo; }
			set { _volumeInfo = value; }
		}

		public bool ExportImages {
			get { return _exportImages; }
			set { _exportImages = value; }
		}
		public ExportNativeWithFilenameFrom ExportNativesToFileNamedFrom {
			get { return _exportNativesToFileNamedFrom; }
			set { _exportNativesToFileNamedFrom = value; }
		}

		public string FilePrefix {
			get { return _filePrefix; }
			set { _filePrefix = value; }
		}

		public ExportedFilePathType TypeOfExportedFilePath {
			get { return _typeOfExportedFilePath; }
			set { _typeOfExportedFilePath = value; }
		}

		public ImageType? TypeOfImage {
			get { return _typeOfImage; }
			set { _typeOfImage = value; }
		}

		public bool AppendOriginalFileName {
			get { return _appendOriginalFileName; }
			set { _appendOriginalFileName = value; }
		}

		public bool LoadFileIsHtml {
			get { return _loadFileIsHtml; }
			set { _loadFileIsHtml = value; }
		}

		[ReadFromExisting()]
		public Types.ViewFieldInfo[] AllExportableFields {
			get { return _allExportableFields; }
			set { _allExportableFields = value; }
		}

		public Types.ViewFieldInfo[] SelectedViewFields {
			get { return _selectedViewFields; }
			set { _selectedViewFields = value; }
		}

		public bool MulticodesAsNested {
			get { return _multicodesAsNested; }
			set { _multicodesAsNested = value; }
		}

		public Types.ViewFieldInfo[] SelectedTextFields {
			get { return _selectedTextFields; }
			set { _selectedTextFields = value; }
		}

		public System.Text.Encoding LoadFileEncoding {
			get { return _loadFileEncoding; }
			set { _loadFileEncoding = value; }
		}

		public System.Text.Encoding TextFileEncoding {
			get { return _textFileEncoding; }
			set { _textFileEncoding = value; }
		}

		public Int32 VolumeDigitPadding {
			get { return _volumeDigitPadding; }
			set { _volumeDigitPadding = value; }
		}

		public Int32 SubdirectoryDigitPadding {
			get { return _subdirectoryDigitPadding; }
			set { _subdirectoryDigitPadding = value; }
		}

		public Int32 StartAtDocumentNumber {
			get { return _startAtDocument; }
			set { _startAtDocument = value; }
		}

		[ReadFromExisting()]
		public DocumentField FileField {
			get { return _fileField; }
			set { _fileField = value; }
		}

		public bool HasFileField {
			get { return (_fileField != null); }
		}

		public string ObjectTypeName { get; set; }

		#endregion

		#region " Serialization "

		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("ArtifactID", this.ArtifactID, typeof(Int32));
			info.AddValue("LoadFilesPrefix", System.Web.HttpUtility.HtmlEncode(this.LoadFilesPrefix), typeof(string));
			info.AddValue("NestedValueDelimiter", Strings.AscW(this.NestedValueDelimiter), typeof(Int32));
			info.AddValue("TypeOfExport", Convert.ToInt32(this.TypeOfExport), typeof(Int32));
			info.AddValue("FolderPath", this.FolderPath, typeof(string));
			info.AddValue("ViewID", this.ViewID, typeof(Int32));
			info.AddValue("Overwrite", this.Overwrite, typeof(bool));
			info.AddValue("RecordDelimiter", Strings.AscW(this.RecordDelimiter), typeof(Int32));
			info.AddValue("QuoteDelimiter", Strings.AscW(this.QuoteDelimiter), typeof(Int32));
			info.AddValue("NewlineDelimiter", Strings.AscW(this.NewlineDelimiter), typeof(Int32));
			info.AddValue("MultiRecordDelimiter", Strings.AscW(this.MultiRecordDelimiter), typeof(Int32));
			info.AddValue("ExportFullText", this.ExportFullText, typeof(bool));
			info.AddValue("ExportFullTextAsFile", this.ExportFullTextAsFile, typeof(bool));
			info.AddValue("ExportNative", this.ExportNative, typeof(bool));
			info.AddValue("LogFileFormat", this.LogFileFormat.HasValue ? Convert.ToInt32(this.LogFileFormat.Value).ToString() : string.Empty, typeof(string));
			info.AddValue("RenameFilesToIdentifier", this.RenameFilesToIdentifier, typeof(bool));
			info.AddValue("IdentifierColumnName", this.IdentifierColumnName, typeof(string));
			info.AddValue("LoadFileExtension", this.LoadFileExtension, typeof(string));
			info.AddValue("ExportImages", this.ExportImages, typeof(bool));
			info.AddValue("ExportNativesToFileNamedFrom", Convert.ToInt32(this.ExportNativesToFileNamedFrom), typeof(Int32));
			info.AddValue("FilePrefix", this.FilePrefix, typeof(string));
			info.AddValue("TypeOfExportedFilePath", Convert.ToInt32(this.TypeOfExportedFilePath), typeof(Int32));
			info.AddValue("TypeOfImage", this.TypeOfImage.HasValue ? Convert.ToInt32(this.TypeOfImage.Value).ToString() : string.Empty, typeof(string));
			info.AddValue("AppendOriginalFileName", this.AppendOriginalFileName, typeof(bool));
			info.AddValue("LoadFileIsHtml", this.LoadFileIsHtml, typeof(bool));
			info.AddValue("MulticodesAsNested", this.MulticodesAsNested, typeof(bool));
			info.AddValue("LoadFileEncoding", this.LoadFileEncoding == null ? -1 : this.LoadFileEncoding.CodePage, typeof(Int32));
			info.AddValue("TextFileEncoding", this.TextFileEncoding == null ? -1 : this.TextFileEncoding.CodePage, typeof(Int32));
			info.AddValue("VolumeDigitPadding", this.VolumeDigitPadding, typeof(Int32));
			info.AddValue("SubdirectoryDigitPadding", this.SubdirectoryDigitPadding, typeof(Int32));
			info.AddValue("StartAtDocumentNumber", this.StartAtDocumentNumber, typeof(Int32));
			info.AddValue("VolumeInfo", this.VolumeInfo, typeof(VolumeInfo));
			info.AddValue("SelectedTextFields", this.SelectedTextFields, typeof(Types.ViewFieldInfo[]));
			info.AddValue("ImagePrecedence", this.ImagePrecedence, typeof(Pair[]));
			info.AddValue("SelectedViewFields", this.SelectedViewFields, typeof(Types.ViewFieldInfo[]));
			info.AddValue("ObjectTypeName", this.ObjectTypeName, typeof(string));
		}

		private ExportFile(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			var _with1 = info;
			this.ArtifactID = info.GetInt32("ArtifactID");
			this.LoadFilesPrefix = System.Web.HttpUtility.HtmlDecode(info.GetString("LoadFilesPrefix"));
			this.NestedValueDelimiter = Strings.ChrW(info.GetInt32("NestedValueDelimiter"));
			this.TypeOfExport = (ExportFile.ExportType)info.GetInt32("TypeOfExport");
			this.FolderPath = info.GetString("FolderPath");
			this.ViewID = info.GetInt32("ViewID");
			this.Overwrite = info.GetBoolean("Overwrite");
			this.RecordDelimiter = Strings.ChrW(info.GetInt32("RecordDelimiter"));
			this.QuoteDelimiter = Strings.ChrW(info.GetInt32("QuoteDelimiter"));
			this.NewlineDelimiter = Strings.ChrW(info.GetInt32("NewlineDelimiter"));
			this.MultiRecordDelimiter = Strings.ChrW(info.GetInt32("MultiRecordDelimiter"));
			this.ExportFullText = info.GetBoolean("ExportFullText");
			this.ExportFullTextAsFile = info.GetBoolean("ExportFullTextAsFile");
			this.ExportNative = info.GetBoolean("ExportNative");
			var _with2 = kCura.Utility.NullableTypesHelper.ToNullableInt32(info.GetString("LogFileFormat"));
			this.LogFileFormat = null;
			if (_with2.HasValue)
				this.LogFileFormat = (LoadFileType.FileFormat)_with2.Value;
			this.RenameFilesToIdentifier = info.GetBoolean("RenameFilesToIdentifier");
			this.IdentifierColumnName = info.GetString("IdentifierColumnName");
			this.LoadFileExtension = info.GetString("LoadFileExtension");
			this.ExportImages = info.GetBoolean("ExportImages");
			this.ExportNativesToFileNamedFrom = (ExportNativeWithFilenameFrom)info.GetInt32("ExportNativesToFileNamedFrom");
			this.FilePrefix = info.GetString("FilePrefix");
			this.TypeOfExportedFilePath = (ExportFile.ExportedFilePathType)info.GetInt32("TypeOfExportedFilePath");
			var _with3 = kCura.Utility.NullableTypesHelper.ToNullableInt32(info.GetString("TypeOfImage"));
			this.TypeOfImage = null;
			if (_with3.HasValue)
				this.TypeOfImage = (ExportFile.ImageType)_with3.Value;
			this.AppendOriginalFileName = info.GetBoolean("AppendOriginalFileName");
			this.LoadFileIsHtml = info.GetBoolean("LoadFileIsHtml");
			this.MulticodesAsNested = info.GetBoolean("MulticodesAsNested");
			Int32 encod = info.GetInt32("LoadFileEncoding");
			this.LoadFileEncoding = encod > 0 ? System.Text.Encoding.GetEncoding(encod) : null;
			encod = info.GetInt32("TextFileEncoding");
			this.TextFileEncoding = encod > 0 ? System.Text.Encoding.GetEncoding(encod) : null;
			this.VolumeDigitPadding = info.GetInt32("VolumeDigitPadding");
			this.SubdirectoryDigitPadding = info.GetInt32("SubdirectoryDigitPadding");
			this.StartAtDocumentNumber = info.GetInt32("StartAtDocumentNumber");
			this.VolumeInfo = (VolumeInfo)info.GetValue("VolumeInfo", typeof(VolumeInfo));
			try {
				this.SelectedTextFields = (Types.ViewFieldInfo[])info.GetValue("SelectedTextFields", typeof(Types.ViewFieldInfo[]));
			} catch {
				object a = info.GetValue("SelectedTextField", typeof(Types.ViewFieldInfo));
				Types.ViewFieldInfo field = (Types.ViewFieldInfo)a;
				this.SelectedTextFields = field == null ? null : new []{ field };
			}
			this.ImagePrecedence = (Pair[])info.GetValue("ImagePrecedence", typeof(Pair[]));
			this.SelectedViewFields = (Types.ViewFieldInfo[])info.GetValue("SelectedViewFields", typeof(Types.ViewFieldInfo[]));
			this.ObjectTypeName = info.GetString("ObjectTypeName");
		}

		#endregion

		#region " Constructors "


		public ExportFile(Int32 artifactTypeID)
		{
			this.RecordDelimiter = Strings.ChrW(20);
			this.QuoteDelimiter = Strings.ChrW(254);
			this.NewlineDelimiter = Strings.ChrW(174);
			this.MultiRecordDelimiter = Strings.ChrW(59);
			this.NestedValueDelimiter = '\\';
			this.MulticodesAsNested = true;
			_artifactTypeID = artifactTypeID;
		}

		#endregion

		#region " Enums "


		public enum ExportType
		{
			Production = 0,
			ArtifactSearch = 1,
			ParentSearch = 2,
			AncestorSearch = 3
		}

		public enum ExportedFilePathType
		{
			Relative = 0,
			Absolute = 1,
			Prefix = 2
		}

		public enum ImageType
		{
			Select = -1,
			SinglePage = 0,
			MultiPageTiff = 1,
			Pdf = 2
		}
		public class ImageTypeParser
		{
			public ImageType? Parse(string s)
			{
				if (string.IsNullOrEmpty(s))
					return null;
				ImageType retval = new ImageType();
				switch (s) {
					case "Single-page TIF/JPG":
						retval = ImageType.SinglePage;
						break;
					case "Multi-page TIF":
						retval = ImageType.MultiPageTiff;
						break;
					case "PDF":
						retval = ImageType.Pdf;
						break;
				}
				return retval;
			}
		}
		#endregion

	}
}
