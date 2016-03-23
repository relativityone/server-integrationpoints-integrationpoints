using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Exports
{
	public class ObjectExportInfo
	{
		private System.Collections.ArrayList _images;
		private object _native;
		private Int64 _totalFileSize;
		private Int64 _totalNumberOfFiles;
		private Int32 _artifactID;
		private bool _hasFullText;
		private string _identifierValue = "";
		private string _nativeExtension = "";
		private string _nativeFileGuid = "";
		private string _nativeTempLocation = "";
		private string _productionBeginBates = "";
		private string _originalFileName = "";
		private string _nativeSourceLocation = "";
		private bool _hasCountedNative = false;
		private bool _hasCountedTextFile = false;
		private Int32 _docCount = 1;
		private Int32 _fileID = 0;

		private object[] _metadata;
		public object[] Metadata {
			get { return _metadata; }
			set { _metadata = value; }
		}

		public System.Collections.ArrayList Images {
			get { return _images; }
			set { _images = value; }
		}

		public object Native {
			get { return _native; }
			set { _native = value; }
		}

		public bool HasFullText {
			get { return _hasFullText; }
			set { _hasFullText = value; }
		}

		public Int64 TotalFileSize {
			get { return _totalFileSize; }
			set { _totalFileSize = value; }
		}

		public Int64 TotalNumberOfFiles {
			get { return _totalNumberOfFiles; }
			set { _totalNumberOfFiles = value; }
		}

		public string IdentifierValue {
			get { return _identifierValue; }
			set { _identifierValue = value; }
		}

		public Int32 ArtifactID {
			get { return _artifactID; }
			set { _artifactID = value; }
		}

		public string NativeExtension {
			get { return _nativeExtension; }
			set { _nativeExtension = value; }
		}

		public string NativeFileGuid {
			get { return _nativeFileGuid; }
			set { _nativeFileGuid = value; }
		}

		public string NativeTempLocation {
			get { return _nativeTempLocation; }
			set { _nativeTempLocation = value; }
		}

		public string NativeSourceLocation {
			get { return _nativeSourceLocation; }
			set { _nativeSourceLocation = value; }
		}

		public string NativeFileName(bool appendToOriginal)
		{
			string retval = null;
			if (appendToOriginal) {
				retval = IdentifierValue + "_" + OriginalFileName;
			} else {
				if (!string.IsNullOrEmpty(NativeExtension)) {
					retval = IdentifierValue + "." + NativeExtension;
				} else {
					retval = IdentifierValue;
				}
			}
			return kCura.Utility.File.Instance.ConvertIllegalCharactersInFilename(retval);
		}

		public string FullTextFileName(bool nameFilesAfterIdentifier)
		{
			string retval = null;
			if (!nameFilesAfterIdentifier) {
				retval = this.ProductionBeginBates;
			} else {
				retval = this.IdentifierValue;
			}
			return kCura.Utility.File.Instance.ConvertIllegalCharactersInFilename(retval + ".txt");
		}

		public string OriginalFileName {
			get { return _originalFileName; }
			set { _originalFileName = value; }
		}

		public string ProductionBeginBatesFileName(bool appendToOriginal) {
		
				string retval = null;
				if (appendToOriginal) {
					retval = ProductionBeginBates + "_" + OriginalFileName;
				} else {
					if (!string.IsNullOrEmpty(NativeExtension)) {
						retval = ProductionBeginBates + "." + NativeExtension;
					} else {
						retval = ProductionBeginBates;
					}
				}
				return kCura.Utility.File.Instance.ConvertIllegalCharactersInFilename(retval);
			
		}

		public Int64 NativeCount {
			get {
				if (string.IsNullOrEmpty(this.NativeFileGuid)) {
					if (!(this.FileID == null) | this.FileID != 0) {
						return 1;
					} else {
						return 0;
					}
				} else {
					return 1;
				}
			}
		}

		public Int64 ImageCount {
			get {
				if (this.Images == null)
					return 0;
				return this.Images.Count;
			}
		}

		public string ProductionBeginBates {
			get { return _productionBeginBates; }
			set { _productionBeginBates = value; }
		}

		public bool HasCountedNative {
			get { return _hasCountedNative; }
			set { _hasCountedNative = value; }
		}
		public bool HasCountedTextFile {
			get { return _hasCountedTextFile; }
			set { _hasCountedTextFile = value; }
		}

		public Int32 FileID {
			get { return _fileID; }
			set { _fileID = value; }
		}


		public Int32 DocCount {
			get {
				Int32 retval = _docCount;
				if (retval == 1)
					_docCount -= 1;
				return retval;
			}
		}
	}
}
