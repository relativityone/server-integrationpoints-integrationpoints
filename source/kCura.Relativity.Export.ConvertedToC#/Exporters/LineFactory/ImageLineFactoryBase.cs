using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.FileObjects;

namespace kCura.Relativity.Export.Exports.LineFactory
{
	public abstract class ImageLineFactoryBase : LineFactoryBase
	{
		private string _batesNumber;
		private Int32 _pageNumber;
		private kCura.Relativity.Export.FileObjects.ExportFile.ImageType _imageType;
		private string _volumeName;

		private string _fullFilePath;

		protected ImageLineFactoryBase(string batesNumber, Int32 pageNumber, string fullFilePath, string volumeName, kCura.Relativity.Export.FileObjects.ExportFile.ImageType imageType)
		{
			_batesNumber = batesNumber;
			_pageNumber = pageNumber;
			_imageType = imageType;
			_volumeName = volumeName;
			_fullFilePath = fullFilePath;
		}

		protected string BatesNumber {
			get { return _batesNumber; }
		}

		protected Int32 PageNumber {
			get { return _pageNumber; }
		}

		protected string VolumeName {
			get { return _volumeName; }
		}

		protected ExportFile.ImageType ImageType {
			get { return _imageType; }
		}

		protected string FullFilePath {
			get { return _fullFilePath; }
		}


	}
}

