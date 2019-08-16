using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	/// <summary>
	/// Returns an object's identifier given its exported value, which in this case is assumed
	/// to be a <see cref="RelativityObjectValue"/>. Import API expects the object identifier instead
	/// of the ArtifactID.
	/// </summary>
	internal sealed class SingleObjectFieldSanitizer : IExportFieldSanitizer
	{
		private readonly ISerializer _serializer;

		public SingleObjectFieldSanitizer(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public string SupportedType => FieldTypeHelper.FieldType.Object.ToString();

		public Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			if (initialValue == null)
			{
				return Task.FromResult((object) null);
			}

			// We have to re-serialize and deserialize the value from Export API due to REL-250554.
			RelativityObjectValue objectValue;
			try
			{
				objectValue = _serializer.Deserialize<RelativityObjectValue>(initialValue.ToString());
			}
			catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier, 
					sanitizingSourceFieldName,
					$"Expected value to be deserializable to {typeof(RelativityObjectValue)}, but instead type was {initialValue.GetType()}.",
					ex);
			}

			// If a Single Object field is not set, Object Manager returns a valid object with an ArtifactID of 0 instead of a null value.
			if (objectValue.ArtifactID == default(int))
			{
				return Task.FromResult<object>(null);
			}

			if (string.IsNullOrWhiteSpace(objectValue.Name))
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier, 
					sanitizingSourceFieldName,
					$"Expected input to be deserializable to type {typeof(RelativityObjectValue)} and name to not be null or empty.");
			}

			string value = objectValue.Name;
			return Task.FromResult<object>(value);
		}
	}
}
