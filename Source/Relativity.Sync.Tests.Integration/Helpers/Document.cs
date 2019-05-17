using System;
using System.Collections.Generic;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed class Document
	{
		public int ArtifactId { get; set; }
		public FieldValue[] Values { get; set; }
		public string WorkspaceFolderPath { get; set; }
		public INativeFile NativeFile { get; set; }

#pragma warning disable RG2011 // Avoid methods with more than 5 arguments, use DTO-style objects or structures for passing multiple arguments
		// Using this method spec. to avoid using DTOs; will make testing easier
		public static Document Create(int artifactId,
			string nativeFileLocation,
			string nativeFileFilename,
			long nativeFileSize,
			string workspaceFolderPath,
			params FieldValue[] fieldValues)
		{
			return new Document
			{
				ArtifactId = artifactId,
				Values = fieldValues,
				WorkspaceFolderPath = workspaceFolderPath,
				NativeFile = new NativeFile(artifactId, nativeFileLocation, nativeFileFilename, nativeFileSize)
			};
		}
#pragma warning restore RG2011 // Avoid methods with more than 5 arguments, use DTO-style objects or structures for passing multiple arguments
	}
}
