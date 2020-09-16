using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal class Dataset
	{
		private Func<FileInfo, string> _begBatesGetter;
		private Func<FileInfo, string> _controlNumberGetter;
		private const string _DATASETS_FOLDER_PATH = "Data";
		private static string CurrentDirectory => AppDomain.CurrentDomain.BaseDirectory;

		public static Dataset NativesAndExtractedText { get; } = new Dataset("NativesAndExtractedText");

		public static Dataset Images { get; } = new Dataset("Images");
		public static Dataset ImagesBig { get; } = new Dataset("ImagesBig");
		public static Dataset ThreeImages { get; } = new Dataset("ThreeImages");
		public static Dataset TwoDocumentProduction { get; } = new Dataset("TwoDocumentProduction");
		public static Dataset SingleDocumentProduction { get; } = new Dataset("SingleDocumentProduction");
		public static Dataset MultipleImagesPerDocument { get; } = new Dataset("MultipleImagesPerDocument", file => "DOCUMENT_1", file => file.Name.Split('_').Last());

		public string Name { get; }
		public string FolderPath => GetDatasetPath(Name);

		protected Dataset(string name, Func<FileInfo, string> controlNumberGetter = null, Func<FileInfo, string> begBatesGetter = null)
		{
			Func<FileInfo, string> GetFilename = file =>
			{
				return Path.GetFileNameWithoutExtension(file.Name);

			};

			_controlNumberGetter = controlNumberGetter ?? GetFilename;
			_begBatesGetter = begBatesGetter ?? GetFilename;
			Name = name;
		}

		public static string GetDatasetPath(string datasetName)
		{
			return Path.Combine(CurrentDirectory, _DATASETS_FOLDER_PATH, datasetName);
		}

		public long GetTotalFilesSize(string fileFilter = "*.*")
		{
			return Directory
				.EnumerateFiles(Path.Combine(CurrentDirectory, _DATASETS_FOLDER_PATH, Name), fileFilter,
					SearchOption.AllDirectories).Select(x => new FileInfo(x).Length).Sum();
		}

		public IEnumerable<FileInfo> GetFiles()
		{
			return Directory.GetFiles(GetDatasetPath(Name)).Select(x => new FileInfo(x));
		}

		public string GetControlNumber(FileInfo file) => _controlNumberGetter(file);
		public string GetBegBates(FileInfo file) => _begBatesGetter(file);
	}
}