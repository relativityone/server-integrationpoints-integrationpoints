using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Newtonsoft.Json;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	/// <summary>
	/// Returns a scalar representation of the selected objects' names given the value of the field,
	/// which is this case is assumed to be an array of <see cref="RelativityObjectValue"/>. Import
	/// API expects a string where each object name is separated by a multi-value delimiter.
	/// </summary>
	internal sealed class MultipleObjectFieldSanitizer : IExportFieldSanitizer
	{
		private readonly ISerializer _serializer;
		private readonly char _multiValueDelimiter;

		public MultipleObjectFieldSanitizer(ISerializer serializer)
		{
			_serializer = serializer;
			_multiValueDelimiter = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;
		}

		public string SupportedType => FieldTypeHelper.FieldType.Objects.ToString();

		public Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			if (initialValue == null)
			{
				return Task.FromResult((object) null);
			}

			// We have to re-serialize and deserialize the value from Export API due to REL-250554.
			RelativityObjectValue[] objectValues;
			try
			{
				objectValues = _serializer.Deserialize<RelativityObjectValue[]>(initialValue.ToString());
			}
			catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier, 
					sanitizingSourceFieldName,
					$"Expected value to be deserializable to {typeof(RelativityObjectValue[])}, but instead type was {initialValue.GetType()}.",
					ex);
			}

			if (objectValues.Any(x => string.IsNullOrWhiteSpace(x.Name)))
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier, 
					sanitizingSourceFieldName,
					$"Expected elements of input to be deserializable to type {typeof(RelativityObjectValue)}.");
			}

			bool ContainsDelimiter(string x) => x.Contains(_multiValueDelimiter);

			List<string> names = objectValues.Select(x => x.Name).ToList();
			if (names.Any(ContainsDelimiter))
			{
				string violatingNameList = string.Join(", ", names.Where(ContainsDelimiter).Select(x => $"'{x}'"));
				throw new IntegrationPointsException(
					$"The identifiers of the following objects referenced by object '{itemIdentifier}' in field '{sanitizingSourceFieldName}' " +
					$"contain the character specified as the multi-value delimiter ('{_multiValueDelimiter}'). Rename these objects or choose " +
					$"a different delimiter: {violatingNameList}.");
			}

			string multiValueDelimiterString = char.ToString(_multiValueDelimiter);
			string combinedNames = string.Join(multiValueDelimiterString, names);
			return Task.FromResult<object>(combinedNames);
		}
	}
}
