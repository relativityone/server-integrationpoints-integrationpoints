using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Newtonsoft.Json;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	/// <summary>
	/// Returns a scalar representation of the selected choices' names given the value of the field,
	/// which is this case is assumed to be an array of <see cref="Choice"/>. Import API expects a
	/// string where each choice is separated by a multi-value delimiter and each parent/child choice
	/// is separated by a nested-value delimiter.
	/// </summary>
	internal sealed class MultipleChoiceFieldSanitizer : IExportFieldSanitizer
	{
		private readonly IChoiceCache _choiceCache;
		private readonly IChoiceTreeToStringConverter _choiceTreeConverter;
		private readonly ISerializer _serializer;
		private readonly char _multiValueDelimiter;
		private readonly char _nestedValueDelimiter;

		public MultipleChoiceFieldSanitizer(IChoiceCache choiceCache, IChoiceTreeToStringConverter choiceTreeConverter, ISerializer serializer)
		{
			_choiceCache = choiceCache;
			_choiceTreeConverter = choiceTreeConverter;
			_serializer = serializer;
			_multiValueDelimiter = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;
			_nestedValueDelimiter = IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER;
		}

		public string SupportedType => FieldTypeHelper.FieldType.MultiCode.ToString();

		public async Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			if (initialValue == null)
			{
				return null;
			}

			// We have to re-serialize and deserialize the value from Export API due to REL-250554.
			Choice[] choices;
			try
			{
				choices = _serializer.Deserialize<Choice[]>(initialValue.ToString());
			}
			catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier, 
					sanitizingSourceFieldName,
					$"Expected value to be deserializable to {typeof(Choice[])}, but instead type was {initialValue.GetType()}.",
					ex);
			}

			if (choices.Any(x => string.IsNullOrWhiteSpace(x.Name)))
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier, 
					sanitizingSourceFieldName,
					$"Expected elements of input to be deserializable to type {typeof(Choice)}.");
			}

			bool ContainsDelimiter(string x) => x.Contains(_multiValueDelimiter) || x.Contains(_nestedValueDelimiter);

			List<string> names = choices.Select(x => x.Name).ToList();
			if (names.Any(ContainsDelimiter))
			{
				string violatingNameList = string.Join(", ", names.Where(ContainsDelimiter).Select(x => $"'{x}'"));
				throw new IntegrationPointsException(
					$"The identifiers of the following choices referenced by object '{itemIdentifier}' in field '{sanitizingSourceFieldName}' " +
					$"contain the character specified as the multi-value delimiter ('{_multiValueDelimiter}') or the one specified as the nested value " +
					$"delimiter ('{_nestedValueDelimiter}'). Rename these choices or choose a different delimiter: {violatingNameList}.");
			}

			IList<ChoiceWithParentInfo> choicesFlatList = await _choiceCache.GetChoicesWithParentInfoAsync(choices).ConfigureAwait(false);
			IList<ChoiceWithChildInfo> choicesTree = BuildChoiceTree(choicesFlatList, null); // start with null to designate the roots of the tree

			string treeString = _choiceTreeConverter.ConvertTreeToString(choicesTree);
			return treeString;
		}

		private IList<ChoiceWithChildInfo> BuildChoiceTree(IList<ChoiceWithParentInfo> flatList, int? parentArtifactID)
		{
			var tree = new List<ChoiceWithChildInfo>();
			foreach (ChoiceWithParentInfo choice in flatList.Where(x => x.ParentArtifactID == parentArtifactID))
			{
				IList<ChoiceWithChildInfo> choiceChildren = BuildChoiceTree(flatList, choice.ArtifactID);
				var choiceWithChildren = new ChoiceWithChildInfo(choice.ArtifactID, choice.Name, choiceChildren);
				tree.Add(choiceWithChildren);
			}
			return tree;
		}
	}
}
