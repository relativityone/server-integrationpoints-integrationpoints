using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class SingleChoiceFieldSanitizer : IExportFieldSanitizer
	{
		public RelativityDataType SupportedType => RelativityDataType.SingleChoice;

		public Task<object> SanitizeAsync(int workspaceArtifactId,
			string itemIdentifierSourceFieldName,
			string itemIdentifier,
			string sanitizingSourceFieldName,
			object initialValue)
		{
			Choice choiceValue = initialValue as Choice;
			if (initialValue != null && choiceValue == null)
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected value of type {typeof(Choice)}, instead was {choiceValue.GetType()}.");
			}

			string value = choiceValue?.Name;
			return Task.FromResult<object>(value);
		}
	}
}
