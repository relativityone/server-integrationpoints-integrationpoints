using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;
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
		private readonly JSONSerializer _serializer = new JSONSerializer();

		public RelativityDataType SupportedType => RelativityDataType.SingleChoice;

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
			Choice choice;
			try
			{
				choice = _serializer.Deserialize<Choice>(initialValue.ToString());
			}
			catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier,
					sanitizingSourceFieldName,
					$"Unable to parse data from Relativity Export API - expected value to be deserializable to {typeof(Choice)}, but instead type was {initialValue.GetType()}",
					ex);
			}

			if (string.IsNullOrWhiteSpace(choice.Name))
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier,
					sanitizingSourceFieldName, 
					$"Unable to parse data from Relativity Export API - expected input to be deserializable to type {typeof(Choice)} and name to not be null or empty");
			}

			string value = choice.Name;
			return Task.FromResult<object>(value);
		}
	}
}
