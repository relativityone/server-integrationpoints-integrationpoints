﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal class Dataset
	{
		private readonly string _elementsCountPath;
		private readonly Func<FileInfo, string> _begBatesGetter;
		private readonly Func<FileInfo, string> _controlNumberGetter;
		private const string _DATASETS_FOLDER_PATH = "Data";
		private static string CurrentDirectory => AppDomain.CurrentDomain.BaseDirectory;

		public static Dataset NativesAndExtractedText { get; } = new Dataset(@"NativesAndExtractedText", elementsCountPath: @"NativesAndExtractedText\natives");

		public static Dataset Images { get; } = new Dataset("Images");
		public static Dataset ImagesBig { get; } = new Dataset("ImagesBig");
		public static Dataset ThreeImages { get; } = new Dataset("ThreeImages");
		public static Dataset TwoDocumentProduction { get; } = new Dataset("TwoDocumentProduction");
		public static Dataset SingleDocumentProduction { get; } = new Dataset("SingleDocumentProduction");
		public static Dataset MultipleImagesPerDocument { get; } = new Dataset("MultipleImagesPerDocument", GetControlNumberForMultipleImages);

		public string Name { get; }
		public string FolderPath => GetDatasetPath(Name);
		public int TotalItemCount => GetFiles(GetDatasetPath(_elementsCountPath)).Count();
		public int TotalDocumentCount  => GetFiles(GetDatasetPath(_elementsCountPath)).GroupBy(_controlNumberGetter).Count();

		private static string GetControlNumberForMultipleImages(FileInfo file)
		{
			return "DOC " + file.Name.Split('_')[1];
		}

		protected Dataset(string name, Func<FileInfo, string> controlNumberGetter = null,
			Func<FileInfo, string> begBatesGetter = null, string elementsCountPath = null)
		{
			_elementsCountPath = elementsCountPath ?? name;
			string GetFilename(FileInfo file) => Path.GetFileNameWithoutExtension(file.Name);

			_controlNumberGetter = controlNumberGetter ?? GetFilename;
			_begBatesGetter = begBatesGetter ?? GetFilename;
			Name = name;
		}

		private static string GetDatasetPath(string filesPath)
		{
			return Path.Combine(CurrentDirectory, _DATASETS_FOLDER_PATH, filesPath);
		}

		public long GetTotalFilesSize(string fileFilter = "*.*")
		{
			return Directory
				.EnumerateFiles(Path.Combine(CurrentDirectory, _DATASETS_FOLDER_PATH, Name), fileFilter,
					SearchOption.AllDirectories).Select(x => new FileInfo(x).Length).Sum();
		}

		public IEnumerable<FileInfo> GetFiles() => GetFiles(GetDatasetPath(Name));

		public IEnumerable<FileInfo> GetFiles(string path)
		{
			return Directory.GetFiles(path).Select(x => new FileInfo(x));
		}

		public string GetControlNumber(FileInfo file) => _controlNumberGetter(file);
		public string GetBegBates(FileInfo file) => _begBatesGetter(file);
	}
}