using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal class Dataset
	{
		private const string _DATASETS_FOLDER_PATH = "Data";
		private static string CurrentDirectory => AppDomain.CurrentDomain.BaseDirectory;

		public static Dataset NativesAndExtractedText { get; } = new Dataset("NativesAndExtractedText");

		public static Dataset Images { get; } = new Dataset("Images");
		public static Dataset ImagesBig { get; } = new Dataset("ImagesBig");
		public static Dataset ThreeImages { get; } = new Dataset("ThreeImages");
		public static Dataset TwoDocumentProduction { get; } = new Dataset("TwoDocumentProduction");
		public static Dataset SingleDocumentProduction { get; } = new Dataset("SingleDocumentProduction");

		public string Name { get; }
		public string FolderPath => GetDatasetPath(Name);

		public Dataset(string name)
		{
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
	}
}