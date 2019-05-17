using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed partial class SourceWorkspaceDataReaderTests
	{
		private static readonly Document[] MultipleBatchesTestData = Enumerable.Range(1, 1000).Select(CreateGenericDocument).ToArray();

		private static Document CreateGenericDocument(int i)
		{
			int artifactId = i;
			string nativeFileLocation = $"\\\\test\\foo\\foo{i}.htm";
			string nativeFileFilename = $"foo{i}.htm";
			long nativeFileSize = 100 + i;
			string workspaceFolderPath = "";
			FieldValue[] fieldValues =
			{
				ControlNumber($"TST{i.ToString("D4", CultureInfo.InvariantCulture)}"),
				RelativityNativeType("Internet HTML"),
				SupportedByViewer(true)
			};

			return Document.Create(artifactId, nativeFileLocation, nativeFileFilename, nativeFileSize, workspaceFolderPath, fieldValues);
		}

		private static FieldValue ControlNumber(string value)
		{
			return new FieldValue("Control Number", value);
		}

		private static FieldValue RelativityNativeType(string value)
		{
			return new FieldValue("RelativityNativeType", value);
		}

		private static FieldValue SupportedByViewer(bool value)
		{
			return new FieldValue("SupportedByViewer", value);
		}
	}
}
