using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Returns an object's identifier given its exported value, which in this case is assumed
	/// to be a <see cref="RelativityObjectValue"/>. Import API expects the object identifier instead
	/// of the ArtifactID.
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
					$"expected value of type {typeof(RelativityObjectValue)}, instead was {initialValue.GetType()}.");
			}

			string value = objectValue?.Name;
			return Task.FromResult<object>(value);
		}
	}
}
