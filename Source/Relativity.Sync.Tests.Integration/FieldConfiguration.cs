using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	internal class FieldConfiguration
	{
		public string SourceColumnName { get; }
		public string DestinationColumnName { get; }
		public RelativityDataType DataType { get; }
		public FieldType Type { get; }
		public ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }
		public object Value { get; }

		private FieldConfiguration(string sourceColumnName, string destinationColumnName, RelativityDataType dataType, FieldType fieldType, object value)
		{
			SourceColumnName = sourceColumnName;
			DestinationColumnName = destinationColumnName;
			DataType = dataType;
			Type = fieldType;
			Value = value;
		}

		public static FieldConfiguration Identifier(string sourceColumnName, string destinationColumnName)
		{
			return new FieldConfiguration(sourceColumnName, destinationColumnName, RelativityDataType.FixedLengthText, FieldType.Identifier, null);
		}

		public static FieldConfiguration Special(string sourceColumnName, string destinationColumnName, RelativityDataType dataType, object value)
		{
			return new FieldConfiguration(sourceColumnName, destinationColumnName, dataType, FieldType.Special, value);
		}

		public static FieldConfiguration Regular(string sourceColumnName, string destinationColumnName, RelativityDataType dataType, object value)
		{
			return new FieldConfiguration(sourceColumnName, destinationColumnName, dataType, FieldType.Regular, value);
		}
	}

	internal enum FieldType
	{
		Identifier,
		Special,
		Regular
	}
}