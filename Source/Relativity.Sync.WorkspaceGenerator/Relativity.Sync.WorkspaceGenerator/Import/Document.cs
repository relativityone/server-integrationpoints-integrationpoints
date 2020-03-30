using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Relativity.Sync.WorkspaceGenerator.LoadFileGenerator
{
	public class Document
	{
		public string Identifier { get; set; } = Guid.NewGuid().ToString();
		public List<Tuple<string, string>> CustomFields { get; set; } = new List<Tuple<string, string>>();
		public FileInfo NativeFile { get; set; }
		public FileInfo ExtractedTextFile { get; set; }
	}
}