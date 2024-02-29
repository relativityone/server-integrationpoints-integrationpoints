using System;
using System.Collections.Generic;
using System.IO;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    public class Document
    {
        public Document(Guid identifierGuid, string testCaseName)
        {
            Identifier = $"{testCaseName}{Consts.ControlNumberSeparator}{identifierGuid}";
            Guid = identifierGuid;
        }

        public string Identifier { get; }
        public List<Tuple<string, string>> CustomFields { get; } = new List<Tuple<string, string>>();
        public FileInfo NativeFile { get; set; }
        public FileInfo ExtractedTextFile { get; set; }
        public Guid Guid { get; }
    }
}