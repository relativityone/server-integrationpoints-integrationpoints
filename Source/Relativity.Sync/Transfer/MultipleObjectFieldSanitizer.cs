using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	internal sealed class MultipleObjectFieldSanitizer : IExportFieldSanitizer
	{
		private readonly ISynchronizationConfiguration _configuration;

		public MultipleObjectFieldSanitizer(ISynchronizationConfiguration configuration)
		{
			_configuration = configuration;
		}

		public RelativityDataType SupportedType => RelativityDataType.MultipleObject;

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

			RelativityObjectValue[] value = initialValue as RelativityObjectValue[];
			if (value == null)
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected value of type {typeof(RelativityObjectValue[])}, instead was {value.GetType()}");
			}

			char multiValueDelimiter = _configuration.ImportSettings.MultiValueDelimiter;

			List<string> names = value.Select(x => x.Name).ToList();
			Func<string, bool> containsDelimiter = x => x.Contains(multiValueDelimiter);
			if (names.Any(containsDelimiter))
			{
				string violatingNameList = string.Join(", ", names.Where(containsDelimiter).Select(x => $"'{x}'"));
				throw new SyncException(
					$"The identifiers of the following objects referenced by object '{itemIdentifier}' in field '{sanitizingSourceFieldName}' " +
					$"contain the character specified as the multi-value delimiter ('{multiValueDelimiter}'). Rename these objects or choose " +
					$"a different delimiter: {violatingNameList}");
			}

			string multiValueDelimiterString = char.ToString(multiValueDelimiter);
			string combinedNames = string.Join(multiValueDelimiterString, names);
			return Task.FromResult<object>(combinedNames);
		}
	}
}
