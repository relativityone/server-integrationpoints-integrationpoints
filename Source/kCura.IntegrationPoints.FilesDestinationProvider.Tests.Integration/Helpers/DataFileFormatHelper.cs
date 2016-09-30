﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
    internal class DataFileFormatHelper
    {
        public static FileInfo GetFileInFormat(string fileFormatExtension, DirectoryInfo directory)
        {
            var files = directory.EnumerateFiles(fileFormatExtension, SearchOption.TopDirectoryOnly).ToList();
            if (files.Count != 1)
            {
                throw new FileNotFoundException("File in given format not found or found more than one file");
            }
            return files.First();
        }

        public static bool FileStartWith(string firstLineStartsWith, FileInfo file)
        {
            using (var reader = new StreamReader(file.FullName))
            {
                var fileFirstLine = reader.ReadLine();
                return fileFirstLine != null && fileFirstLine.StartsWith(firstLineStartsWith);
            }
        }

	    public static bool LineNumberContains(int lineNumber, string text, FileInfo file)
	    {
			using (var reader = new StreamReader(file.FullName))
			{
				for (int i = 1; i < lineNumber; i++)
					reader.ReadLine();

				var line = reader.ReadLine();
				return line != null && line.Contains(text);
			}
		}

		public static IEnumerable<T> GetMetadataFileColumnValues<T>(string columnName, FileInfo file, char colSeparator = ',')
		{
			using (var reader = new StreamReader(file.FullName))
			{
				var line = reader.ReadLine();

				if (line == null)
				{
					yield break;
				}
				var index = line.Split(colSeparator).ToList().FindIndex(item => item.Contains(columnName));
				while ((line = reader.ReadLine()) != null)
				{
					var lineValues = line.Split(colSeparator).ToList();
					var value = lineValues[index].Remove(lineValues[index].Length - 1).Remove(0, 1);
					yield return (T)Convert.ChangeType(value, typeof(T));
				}
			}
		}
	}
}