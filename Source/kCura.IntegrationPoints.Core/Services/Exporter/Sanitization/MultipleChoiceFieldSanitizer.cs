﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	/// <summary>
	/// Returns a scalar representation of the selected choices' names given the value of the field,
	/// which is this case is assumed to be an array of <see cref="ChoiceDto"/>. Import API expects a
	/// string where each choice is separated by a multi-value delimiter and each parent/child choice
	/// is separated by a nested-value delimiter.
	/// </summary>
	internal sealed class MultipleChoiceFieldSanitizer : IExportFieldSanitizer
	{
		private readonly IChoiceCache _choiceCache;
		private readonly IChoiceTreeToStringConverter _choiceTreeConverter;
		private readonly ISanitizationHelper _sanitizationHelper;
		private readonly char _multiValueDelimiter;
		private readonly char _nestedValueDelimiter;

		public MultipleChoiceFieldSanitizer(IChoiceCache choiceCache, IChoiceTreeToStringConverter choiceTreeConverter, ISanitizationHelper sanitizationHelper)
		{
			_choiceCache = choiceCache;
			_choiceTreeConverter = choiceTreeConverter;
			_sanitizationHelper = sanitizationHelper;
			_multiValueDelimiter = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;
			_nestedValueDelimiter = IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER;
		}

		public FieldTypeHelper.FieldType SupportedType => FieldTypeHelper.FieldType.MultiCode;

		public async Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			if (initialValue == null)
			{
				return null;
			}

			// We have to re-serialize and deserialize the value from Export API due to REL-250554.
			ChoiceDto[] choices = _sanitizationHelper.DeserializeAndValidateExportFieldValue<ChoiceDto[]>(
				itemIdentifier, 
				sanitizingSourceFieldName, 
				initialValue);

			if (choices.Any(x => string.IsNullOrWhiteSpace(x.Name)))
			{
				throw new InvalidExportFieldValueException(
					itemIdentifier, 
					sanitizingSourceFieldName,
					$"Expected elements of input to be deserializable to type {typeof(ChoiceDto)}.");
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

			IList<ChoiceWithParentInfoDto> choicesFlatList = await _choiceCache.GetChoicesWithParentInfoAsync(choices).ConfigureAwait(false);
			IList<ChoiceWithChildInfoDto> choicesTree = BuildChoiceTree(choicesFlatList, null); // start with null to designate the roots of the tree

			string treeString = _choiceTreeConverter.ConvertTreeToString(choicesTree);
			return treeString;
		}

		private IList<ChoiceWithChildInfoDto> BuildChoiceTree(IList<ChoiceWithParentInfoDto> flatList, int? parentArtifactID)
		{
			var tree = new List<ChoiceWithChildInfoDto>();
			foreach (ChoiceWithParentInfoDto choice in flatList.Where(x => x.ParentArtifactID == parentArtifactID))
			{
				IList<ChoiceWithChildInfoDto> choiceChildren = BuildChoiceTree(flatList, choice.ArtifactID);
				var choiceWithChildren = new ChoiceWithChildInfoDto(choice.ArtifactID, choice.Name, choiceChildren);
				tree.Add(choiceWithChildren);
			}
			return tree;
		}
	}
}
