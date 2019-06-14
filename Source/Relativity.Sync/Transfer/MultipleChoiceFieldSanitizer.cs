using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	internal sealed class MultipleChoiceFieldSanitizer : IExportFieldSanitizer
	{
		private readonly ISynchronizationConfiguration _configuration;

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

			Choice[] value = initialValue as Choice[];
			if (value == null)
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected value of type {typeof(Choice[])}, instead was {value.GetType()}");
			}

			char multiValueDelimiter = _configuration.ImportSettings.MultiValueDelimiter;
			char nestedValueDelimiter = _configuration.ImportSettings.NestedValueDelimiter;

			List<string> names = value.Select(x => x.Name).ToList();
			Func<string, bool> containsDelimiter = x => x.Contains(multiValueDelimiter) || x.Contains(nestedValueDelimiter);
			if (names.Any(containsDelimiter))
			{
				string violatingNameList = string.Join(", ", names.Where(containsDelimiter).Select(x => $"'{x}'"));
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
