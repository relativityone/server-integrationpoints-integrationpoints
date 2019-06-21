using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

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
		private readonly ISynchronizationConfiguration _configuration;
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly IChoiceTreeToStringConverter _choiceTreeConverter;
		private readonly JSONSerializer _serializer = new JSONSerializer();

		public MultipleChoiceFieldSanitizer(ISynchronizationConfiguration configuration, ISourceServiceFactoryForAdmin serviceFactory, IChoiceTreeToStringConverter choiceTreeConverter)
		{
			_configuration = configuration;
			_serviceFactory = serviceFactory;
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
				throw new InvalidExportFieldValueException(itemIdentifier, sanitizingSourceFieldName,
					$"Expected value to be deserializable to {typeof(Choice[])}, but instead type was {initialValue.GetType()}.",
					ex);
			}

			if (choices.Any(x => string.IsNullOrWhiteSpace(x.Name)))
			{
				throw new InvalidExportFieldValueException(itemIdentifier, sanitizingSourceFieldName,
					$"Expected elements of input to be deserializable to type {typeof(Choice)}.");
			}

			char multiValueDelimiter = _configuration.ImportSettings.MultiValueDelimiter;
			char nestedValueDelimiter = _configuration.ImportSettings.NestedValueDelimiter;
			bool ContainsDelimiter(string x) => x.Contains(multiValueDelimiter) || x.Contains(nestedValueDelimiter);

			List<string> names = choices.Select(x => x.Name).ToList();
			if (names.Any(ContainsDelimiter))
			{
				string violatingNameList = string.Join(", ", names.Where(ContainsDelimiter).Select(x => $"'{x}'"));
				throw new SyncException(
					$"The identifiers of the following choices referenced by object '{itemIdentifier}' in field '{sanitizingSourceFieldName}' " +
					$"contain the character specified as the multi-value delimiter ('{multiValueDelimiter}') or the one specified as the nested value " +
					$"delimiter ('{nestedValueDelimiter}'). Rename these choices or choose a different delimiter: {violatingNameList}.");
			}

			IList<ChoiceWithParentInfo> choicesFlatList = await GetChoicesWithParentInfoAsync(choices).ConfigureAwait(false);
			const int parentArtifactIdForRootChoices = 1003663; // TODO get parent artifact ID (which is workspace id) of root choices
			IList<ChoiceWithParentInfo> choicesTree = BuildChoiceTree(choicesFlatList, parentArtifactIdForRootChoices);

			string treeString = _choiceTreeConverter.ConvertTreeToString(choicesTree);
			return treeString;
		}

		private async Task<IList<ChoiceWithParentInfo>> GetChoicesWithParentInfoAsync(Choice[] choices)
		{
			IList<ChoiceWithParentInfo> choicesWithParentInfo = new List<ChoiceWithParentInfo>(choices.Length);
			using (IObjectManager om = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				foreach (Choice choice in choices)
				{
					const int choiceArtifactTypeId = 7;
					QueryRequest request = new QueryRequest()
					{
						ObjectType = new ObjectTypeRef()
						{
							ArtifactTypeID = choiceArtifactTypeId
						},
						Condition = $"'ArtifactID' == {choice.ArtifactID}"
					};
					QueryResult queryResult = await om.QueryAsync(_configuration.SourceWorkspaceArtifactId, request, 1, 1).ConfigureAwait(false);
					if (queryResult.ResultCount == 0)
					{
						throw new SyncException($"Query for Choice Artifact ID: {choice.ArtifactID} returned no results.");
					}

					int parentArtifactId = queryResult.Objects[0].ParentObject.ArtifactID;
					
					choicesWithParentInfo.Add(new ChoiceWithParentInfo(parentArtifactId, choice.ArtifactID, choice.Name));
				}
			}

			return choicesWithParentInfo;
		}

		private IList<ChoiceWithParentInfo> BuildChoiceTree(IList<ChoiceWithParentInfo> flatList, int parentArtifactId)
		{
			IList<ChoiceWithParentInfo> tree = new List<ChoiceWithParentInfo>();
			foreach (ChoiceWithParentInfo choice in flatList.Where(x => x.ParentArtifactID == parentArtifactId))
			{
				choice.Children = BuildChoiceTree(flatList, choice.ArtifactID);
				tree.Add(choice);
			}
			return tree;
		}
	}
}