using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Returns a scalar representation of the selected choices' names given the value of the field,
	/// which is this case is assumed to be an array of <see cref="Choice"/>. Import API expects a
	/// string where each choice is separated by a multi-value delimiter and each parent/child choice
	/// is separated by a nested-value delimiter.
	/// </summary>
	internal sealed class MultipleChoiceFieldSanitizer : IExportFieldSanitizer
	{
		private readonly IDocumentSynchronizationConfiguration _configuration;
		private readonly IChoiceCache _choiceCache;
		private readonly IChoiceTreeToStringConverter _choiceTreeConverter;
		private readonly JSONSerializer _serializer = new JSONSerializer();

		public MultipleChoiceFieldSanitizer(IDocumentSynchronizationConfiguration configuration, IChoiceCache choiceCache, IChoiceTreeToStringConverter choiceTreeConverter)
		{
			_configuration = configuration;
			_choiceCache = choiceCache;
			_choiceTreeConverter = choiceTreeConverter;
		}

		public RelativityDataType SupportedType => RelativityDataType.MultipleChoice;

		public async Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			if (initialValue == null)
			{
				return initialValue;
			}

			// We have to re-serialize and deserialize the value from Export API due to REL-250554.
			Choice[] choices;
			try
			{
				choices = _serializer.Deserialize<Choice[]>(initialValue.ToString());
			}
			catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
			{
				throw new InvalidExportFieldValueException($"Expected value to be deserializable to {typeof(Choice[])}, but instead type was {initialValue.GetType()}.",
					ex);
			}

			if (choices.Any(x => string.IsNullOrWhiteSpace(x.Name)))
			{
				throw new InvalidExportFieldValueException($"One or more choices are null or contain only white-space characters.");
			}

			char multiValueDelimiter = _configuration.MultiValueDelimiter;
			char nestedValueDelimiter = _configuration.NestedValueDelimiter;
			bool ContainsDelimiter(string x) => x.Contains(multiValueDelimiter) || x.Contains(nestedValueDelimiter);

			List<string> names = choices.Select(x => x.Name).ToList();
			if (names.Any(ContainsDelimiter))
			{
				throw new InvalidExportFieldValueException(
					$"The identifiers of the choices contain the character specified as the multi-value delimiter ('ASCII {(int)multiValueDelimiter}')" +
					$" or nested value delimiter ('ASCII {(int)nestedValueDelimiter}'). Rename choices to not contain delimiters.");
			}

			IList<ChoiceWithParentInfo> choicesFlatList = await _choiceCache.GetChoicesWithParentInfoAsync(choices).ConfigureAwait(false);
			IList<ChoiceWithChildInfo> choicesTree = BuildChoiceTree(choicesFlatList, null); // start with null to designate the roots of the tree

			string treeString = _choiceTreeConverter.ConvertTreeToString(choicesTree);
			return treeString;
		}

		private IList<ChoiceWithChildInfo> BuildChoiceTree(IList<ChoiceWithParentInfo> flatList, int? parentArtifactId)
		{
			var tree = new List<ChoiceWithChildInfo>();
			foreach (ChoiceWithParentInfo choice in flatList.Where(x => x.ParentArtifactId == parentArtifactId))
			{
				IList<ChoiceWithChildInfo> choiceChildren = BuildChoiceTree(flatList, choice.ArtifactID);
				var choiceWithChildren = new ChoiceWithChildInfo(choice.ArtifactID, choice.Name, choiceChildren);
				tree.Add(choiceWithChildren);
			}
			return tree;
		}
	}
}