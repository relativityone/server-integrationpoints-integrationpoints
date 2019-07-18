using System;
using System.IO;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal class Dataset
	{
		private const string _DATASETS_FOLDER_PATH = "Data";
		private static string CurrentDirectory => AppDomain.CurrentDomain.BaseDirectory;

		public static Dataset NativesAndExtractedText { get; } = new Dataset("NativesAndExtractedText");

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
	}
}