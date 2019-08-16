using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	/// <summary>
	/// Returns a choice's name given its exported value, which in this case is assumed
	/// to be a <see cref="Choice"/>. Import API expects the choice name instead of e.g.
	/// its artifact ID.
	/// </summary>
	internal sealed class SingleChoiceFieldSanitizer : IExportFieldSanitizer
	{
		private readonly ISerializer _serializer;

		public SingleChoiceFieldSanitizer(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public string SupportedType => FieldTypeHelper.FieldType.Code.ToString();

		public Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			if (initialValue == null)
			{
				return Task.FromResult((object) null);
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
					$"Expected value to be deserializable to {typeof(Choice)}, but instead type was {initialValue.GetType()}.",
					ex);
			}

			if (string.IsNullOrWhiteSpace(choice.Name))
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier, 
					sanitizingSourceFieldName,
					$"Expected input to be deserializable to type {typeof(Choice)} and name to not be null or empty.");
			}

			string value = choice.Name;
			return Task.FromResult<object>(value);
		}
	}
}
