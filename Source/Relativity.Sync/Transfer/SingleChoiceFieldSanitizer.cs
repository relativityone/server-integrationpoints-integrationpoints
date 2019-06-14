using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Returns a choice's name given its exported value, which in this case is assumed
	/// to be a <see cref="Choice"/>. Import API expects the choice name instead of e.g.
	/// its artifact ID.
	/// </summary>
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
					$"expected value of type {typeof(Choice)}, instead was {initialValue.GetType()}.");
			}

			string value = choiceValue?.Name;
			return Task.FromResult<object>(value);
		}
	}
}
