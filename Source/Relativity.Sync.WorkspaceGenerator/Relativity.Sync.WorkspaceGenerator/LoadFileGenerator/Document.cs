using System;
using System.IO;

namespace Relativity.Sync.WorkspaceGenerator.LoadFileGenerator
{
	public class Document
	{
		public Document(FileInfo nativeFile, FileInfo extractedTextFile)
		{
			Identifier = Guid.NewGuid().ToString();
			NativeFile = nativeFile;
			ExtractedTextFile = extractedTextFile;
		}

		public string Identifier { get; set; }
		public FileInfo NativeFile { get; set; }
		public FileInfo ExtractedTextFile { get; set; }
	}
}