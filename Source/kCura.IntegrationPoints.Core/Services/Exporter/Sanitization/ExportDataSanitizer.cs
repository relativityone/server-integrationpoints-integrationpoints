using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal class ExportDataSanitizer : IExportDataSanitizer
	{
		private readonly Dictionary<string, IExportFieldSanitizer> _sanitizers;

		public ExportDataSanitizer(IExportFieldSanitizerProvider sanitizerProvider)
		{
			IList<IExportFieldSanitizer> sanitizersList =
				sanitizerProvider.GetExportFieldSanitizers() ?? new List<IExportFieldSanitizer>();
			HashSet<string> uniqueDataTypes = new HashSet<string>(sanitizersList.Select(x => x.SupportedType));
			if (sanitizersList.Count > uniqueDataTypes.Count)
			{
				throw new IntegrationPointsException(
					"Multiple sanitizers for the same data type were found. Ensure there is exactly one sanitizer per data type.");
			}

			_sanitizers = sanitizersList.ToDictionary(s => s.SupportedType);
		}

		public bool ShouldSanitize(string dataType)
		{
			return _sanitizers.ContainsKey(dataType);
		}

		public async Task<object> SanitizeAsync(
			int workspaceArtifactID, 
			string itemIdentifierSourceFieldName,
			string itemIdentifier,
			string fieldName,
			string fieldType,
			object initialValue)
		{
			if (!_sanitizers.ContainsKey(fieldType))
			{
				throw new InvalidOperationException($"No field sanitizer found for given '{fieldType}' data type.");
			}

			return await _sanitizers[fieldType]
				.SanitizeAsync(workspaceArtifactID, itemIdentifierSourceFieldName, itemIdentifier, fieldName, initialValue)
				.ConfigureAwait(false);
		}
	}
}
