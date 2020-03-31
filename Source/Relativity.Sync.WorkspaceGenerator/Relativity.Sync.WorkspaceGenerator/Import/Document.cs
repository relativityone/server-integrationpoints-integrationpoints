using System;
using System.Collections.Generic;
using System.IO;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public class Document
	{
		public Document(string identifier)
		{
			Identifier = identifier;
		}

		public string Identifier { get; set; }
		public List<Tuple<string, string>> CustomFields { get; set; } = new List<Tuple<string, string>>();
		public FileInfo NativeFile { get; set; }
		public FileInfo ExtractedTextFile { get; set; }
	}
}