using System;
using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace Rip.E2ETests.CustomProviders.TestCases
{
	internal class CustomProviderTestCase
	{
		public string CustomProviderName { get; set; }
		public string InputFilePath { get; set; }
		public string FieldFilePath { get; set; }
		public string DataFilePath { get; set; }
		public string TargetRdoArtifactName { get; set; }
		public string IdentifierFieldName { get; set; }
		public Dictionary<string, string> WorkspaceFieldsToFileFieldsMapping { get; set; }
		public Dictionary<string, string> SourceFieldFieldIdentifierToDisplayNameMapping { get; set; }
		public TimeSpan MaximumExecutionTime { get; set; }
		public Choice ExpectedStatus { get; set; }
		public int ExpectedTotalItems { get; set; }
		public int ExpectedItemsTransferred { get; set; }
		public Dictionary<string, string> ExpectedDocumentsIdentifiersToExtractedTextMapping { get; set; }
		public IEnumerable<string> WorkspaceFieldsNames => WorkspaceFieldsToFileFieldsMapping.Keys;

		public IEnumerable<string> ExpectedDocumentsIdentifiers =>
			ExpectedDocumentsIdentifiersToExtractedTextMapping.Keys;
	}
}