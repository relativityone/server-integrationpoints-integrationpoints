using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;
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
		private readonly JSONSerializer _serializer = new JSONSerializer();

		public RelativityDataType SupportedType => RelativityDataType.SingleObject;

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
			RelativityObjectValue objectValue;
			try
			{
				objectValue = _serializer.Deserialize<RelativityObjectValue>(initialValue.ToString());
			}
			catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected value to be deserializable to {typeof(RelativityObjectValue)}, but instead type was {initialValue.GetType()}", ex);
			}

			if (string.IsNullOrWhiteSpace(objectValue.Name))
			{
				throw new SyncException("Unable to parse data from Relativity Export API - " +
					$"expected input to be deserializable to type {typeof(RelativityObjectValue)} and name to not be null or empty (object value was: {initialValue})");
			}

			string value = objectValue.Name;
			return Task.FromResult<object>(value);
		}
	}
}
