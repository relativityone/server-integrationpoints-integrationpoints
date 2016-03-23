using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Exports
{
	public class ImageExportInfo
	{
		private string _fileName;
		private string _fileGuid;
		private Int32 _artifactID;
		private string _batesNumber;
		private string _tempLocation;
		private string _sourceLocation;
		private Nullable<Int32> _pageOffset;

		private bool _hasBeenDownloaded = false;
		public string FileName {
			get { return _fileName; }
			set { _fileName = value; }
		}

		public string FileGuid {
			get { return _fileGuid; }
			set { _fileGuid = value; }
		}

		public Int32 ArtifactID {
			get { return _artifactID; }
			set { _artifactID = value; }
		}

		public string BatesNumber {
			get { return _batesNumber; }
			set { _batesNumber = value; }
		}

		public string TempLocation {
			get { return _tempLocation; }
			set { _tempLocation = value; }
		}

		public string SourceLocation {
			get { return _sourceLocation; }
			set { _sourceLocation = value; }
		}

		public Nullable<Int32> PageOffset {
			get { return _pageOffset; }
			set { _pageOffset = value; }
		}

		public bool HasBeenCounted {
			get { return _hasBeenDownloaded; }
			set { _hasBeenDownloaded = value; }
		}

	}
}
