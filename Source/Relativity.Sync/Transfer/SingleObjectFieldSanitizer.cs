using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Assumes that the incoming initial value is an artifact ID, and queries for the corresponding object's identifier
	/// and returns that instead. Import API requires the 
	/// </summary>
	internal sealed class SingleObjectFieldSanitizer : IExportFieldSanitizer
	{
		public RelativityDataType SupportedType => RelativityDataType.SingleObject;

		public Task<object> SanitizeAsync(int workspaceArtifactId,
			string itemIdentifierSourceFieldName,
			string itemIdentifier,
			string sanitizingSourceFieldName,
			object initialValue)
		{
			RelativityObjectValue objectValue = initialValue as RelativityObjectValue;
			if (initialValue != null && objectValue == null)
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected value of type {typeof(RelativityObjectValue)}, instead was {objectValue.GetType()}.");
			}

			string value = objectValue?.Name;
			return Task.FromResult<object>(value);
		}
	}
}
