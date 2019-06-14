using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Returns a scalar representation of the selected objects' names given the value of the field,
	/// which is this case is assumed to be an array of <see cref="RelativityObjectValue"/>. Import
	/// API expects a string where each object name is separated by a multi-value delimiter.
	/// </summary>
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

			if (!(initialValue is RelativityObjectValue[] value))
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected value of type {typeof(RelativityObjectValue[])}, instead was {initialValue.GetType()}");
			}

			char multiValueDelimiter = _configuration.ImportSettings.MultiValueDelimiter;
			bool ContainsDelimiter(string x) => x.Contains(multiValueDelimiter);

			List<string> names = value.Select(x => x.Name).ToList();
			if (names.Any(ContainsDelimiter))
			{
				string violatingNameList = string.Join(", ", names.Where(ContainsDelimiter).Select(x => $"'{x}'"));
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
