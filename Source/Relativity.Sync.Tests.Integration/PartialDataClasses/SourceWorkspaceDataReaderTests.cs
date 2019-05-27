using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed partial class SourceWorkspaceDataReaderTests
	{
		private static readonly Dictionary<string, RelativityDataType> StandardSchema = new Dictionary<string, RelativityDataType>
		{
			{ "Control Number", RelativityDataType.FixedLengthText },
			{ "RelativityNativeType", RelativityDataType.FixedLengthText },
			{ "SupportedByViewer", RelativityDataType.YesNo },
		};

		private static readonly IList<FieldMap> StandardFieldMappings = new List<FieldMap>
		{
			new FieldMap
			{
				FieldMapType = FieldMapType.None,
				DestinationField = new FieldEntry { DisplayName = "Control Number" },
				SourceField = new FieldEntry { DisplayName = "Control Number", IsIdentifier = true }
			}
		};

		private static readonly DocumentImportJob MultipleBatchesImportJob = DocumentImportJob.Create(
			StandardSchema,
			StandardFieldMappings,
			Enumerable.Range(1, 1000).Select(CreateDocumentForStandardSchema).ToArray());

		private static Document CreateDocumentForStandardSchema(int i)
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
