using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Process
{
	public class Statistics
	{
		private Int64 _metadataBytes = 0;
		private Int64 _metadataTime = 0;
		private Int64 _fileBytes = 0;
		private Int64 _fileTime = 0;
		private Int64 _sqlTime = 0;
		private Int64 _docCount = 0;
		private System.DateTime _lastAccessed;
		private Int32 _documentsCreated = 0;
		private Int32 _documentsUpdated = 0;

		private Int32 _filesProcessed = 0;
		public Int32 BatchSize { get; set; }

		public Int64 MetadataBytes {
			get { return _metadataBytes; }
			set {
				_lastAccessed = System.DateTime.Now;
				_metadataBytes = value;
			}
		}

		public Int64 MetadataTime {
			get { return _metadataTime; }
			set {
				_lastAccessed = System.DateTime.Now;
				_metadataTime = value;
			}
		}

		public Int64 FileBytes {
			get { return _fileBytes; }
			set {
				_lastAccessed = System.DateTime.Now;
				_fileBytes = value;
			}
		}

		public Int64 FileTime {
			get { return _fileTime; }
			set {
				_lastAccessed = System.DateTime.Now;
				_fileTime = value;
			}
		}

		public Int64 SqlTime {
			get { return _sqlTime; }
			set {
				_lastAccessed = System.DateTime.Now;
				_sqlTime = value;
			}
		}

		public Int64 DocCount {
			get { return _docCount; }
			set {
				_lastAccessed = System.DateTime.Now;
				_docCount = value;
			}
		}

		public System.DateTime LastAccessed {
			get { return _lastAccessed; }
		}

		public string ToFileSizeSpecification(double value)
		{
			string prefix = null;
			Int32 k = default(Int32);
			if (value <= 0) {
				k = 0;
			} else {
				k = (Int32)System.Math.Floor(System.Math.Log(value, 1000));
			}
			switch (k) {
				case 0:
					prefix = "";
					break;
				case 1:
					prefix = "K";
					break;
				case 2:
					prefix = "M";
					break;
				case 3:
					prefix = "G";
					break;
				case 4:
					prefix = "T";
					break;
				case 5:
					prefix = "P";
					break;
				case 6:
					prefix = "E";
					break;
				case 7:
					prefix = "B";
					break;
				case 8:
					prefix = "Y";
					break;
			}
			return (value / Math.Pow(1000, k)).ToString("N2") + " " + prefix + "B";
		}

		public Int32 DocumentsCreated {
			get { return _documentsCreated; }
		}

		public Int32 DocumentsUpdated {
			get { return _documentsUpdated; }
		}

		public Int32 FilesProcessed {
			get { return _filesProcessed; }
		}

		//Public Sub ProcessRunResults(ByVal results As kCura.EDDS.WebAPI.BulkImportManagerBase.MassImportResults)
		//	_documentsCreated += results.ArtifactsCreated
		//	_documentsUpdated += results.ArtifactsUpdated
		//	_filesProcessed += results.FilesProcessed
		//End Sub


		public virtual IDictionary ToDictionary()
		{
			System.Collections.Specialized.HybridDictionary retval = new System.Collections.Specialized.HybridDictionary();
			if (!(this.FileTime == 0))
				retval.Add("Average file transfer rate", ToFileSizeSpecification(this.FileBytes / (this.FileTime / 10000000)) + "/sec");
			if (!(this.MetadataTime == 0))
				retval.Add("Average metadata transfer rate", ToFileSizeSpecification(this.MetadataBytes / (this.MetadataTime / 10000000)) + "/sec");
			if (!(this.SqlTime == 0))
				retval.Add("Average SQL process rate", (this.DocCount / (this.SqlTime / 10000000)).ToString("N0") + " Documents/sec");
			if (!(this.BatchSize == 0))
				retval.Add("Current batch size", (this.BatchSize).ToString("N0"));
			return retval;
		}
	}
}
