using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.FileObjects;

namespace kCura.Relativity.Export.Exports.LineFactory
{
	public class SimpleIproImageLineFactory : ImageLineFactoryBase
	{

		#region "Constructors"

		public SimpleIproImageLineFactory(string batesNumber, Int32 pageNumber, string fullFilePath, string volumeName, kCura.Relativity.Export.FileObjects.ExportFile.ImageType imageType) : base(batesNumber, pageNumber, fullFilePath, volumeName, imageType)
		{
		}

		#endregion

		#region "Cell Contents"

		private string ImportCodeIdentifier {
			get { return "IM"; }
		}

		private string ImageKey {
			get { return this.BatesNumber; }
		}

		private string DocumentDesignation {
			get {
				if (this.PageNumber == 1) {
					return "D";
				} else {
					return "";
				}
			}
		}

		private string TiffFileOffset {
			get {
				switch (this.ImageType) {
					case ExportFile.ImageType.MultiPageTiff:
						return this.PageNumber.ToString();
					case ExportFile.ImageType.Pdf:
						return this.PageNumber.ToString();
					case ExportFile.ImageType.SinglePage:
						return 0.ToString();
				}
				return null;
			}
		}

		private string VolumeIdentifier {
			get { return "@" + this.VolumeName; }
		}

		private string DirectoryPath {
			get { return System.IO.Path.GetDirectoryName(this.FullFilePath); }
		}

		private string Filename {
			get { return System.IO.Path.GetFileName(this.FullFilePath); }
		}

		private string IproImageFileType {
			get {
				switch (System.IO.Path.GetExtension(this.FullFilePath).ToLower().Trim(".".ToCharArray())) {
					case "pdf":
						return 7.ToString();
					case "jpg":
					case "jpeg":
						return 4.ToString();
					case "tif":
					case "tiff":
						return 2.ToString();
				}
				return null;
			}
		}

		#endregion

		#region "Virtual Method Implementation"

		public override void WriteLine(System.IO.StreamWriter stream)
		{
			stream.Write(this.ImportCodeIdentifier);
			stream.Write(",");
			stream.Write(this.ImageKey);
			stream.Write(",");
			stream.Write(this.DocumentDesignation);
			stream.Write(",");
			stream.Write(this.TiffFileOffset);
			stream.Write(",");
			stream.Write(this.VolumeIdentifier);
			stream.Write(";");
			stream.Write(this.DirectoryPath);
			stream.Write(";");
			stream.Write(this.Filename);
			stream.Write(";");
			stream.Write(this.IproImageFileType);
			stream.Write(Constants.vbNewLine);
		}

		#endregion

	}
}

