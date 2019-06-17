using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Returns a scalar representation of the selected choices' names given the value of the field,
	/// which is this case is assumed to be an array of <see cref="Choice"/>. Import API expects a
	/// string where each choice is separated by a multi-value delimiter and each parent/child choice
	/// is separated by a nested-value delimiter.
	/// </summary>
	internal sealed class MultipleChoiceFieldSanitizer : IExportFieldSanitizer
	{
		private readonly ISynchronizationConfiguration _configuration;
		private readonly JSONSerializer _serializer = new JSONSerializer();

		public MultipleChoiceFieldSanitizer(ISynchronizationConfiguration configuration)
		{
			_configuration = configuration;
		}

		public RelativityDataType SupportedType => RelativityDataType.MultipleChoice;

		public Task<object> SanitizeAsync(int workspaceArtifactId,
			string itemIdentifierSourceFieldName,
			string itemIdentifier,
			string sanitizingSourceFieldName,
			object initialValue)
		{
			if (initialValue == null)
			{
				return Task.FromResult(initialValue);
			}

			// We have to re-serialize and deserialize the value from Export API due to REL-250554.
			Choice[] choices;
			try
			{
				choices = _serializer.Deserialize<Choice[]>(initialValue.ToString());
			}
			catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected value to be deserializable to {typeof(Choice[])}, but instead type was {initialValue.GetType()}", ex);
			}

			if (choices.Any(x => string.IsNullOrWhiteSpace(x.Name)))
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected elements of input to be deserializable to type {typeof(Choice)}");
			}

			char multiValueDelimiter = _configuration.ImportSettings.MultiValueDelimiter;
			char nestedValueDelimiter = _configuration.ImportSettings.NestedValueDelimiter;
			bool ContainsDelimiter(string x) => x.Contains(multiValueDelimiter) || x.Contains(nestedValueDelimiter);

			List<string> names = choices.Select(x => x.Name).ToList();
			if (names.Any(ContainsDelimiter))
			{
				string violatingNameList = string.Join(", ", names.Where(ContainsDelimiter).Select(x => $"'{x}'"));
				throw new SyncException(
					$"The identifiers of the following choices referenced by object '{itemIdentifier}' in field '{sanitizingSourceFieldName}' " +
					$"contain the character specified as the multi-value delimiter ('{multiValueDelimiter}') or the one specified as the nested value " +
					$"delimiter ('{nestedValueDelimiter}'). Rename these choices or choose a different delimiter: {violatingNameList}");
			}

			// TODO: Check & combine based on choice nesting
			string multiValueDelimiterString = char.ToString(multiValueDelimiter);
			string combinedNames = string.Join(multiValueDelimiterString, names);
			return Task.FromResult<object>(combinedNames);
		}
	}
}
