using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ChoiceService : IChoiceService
	{
		private readonly IChoiceQuery _query;
		public ChoiceService(IChoiceQuery query)
		{
			_query = query;
		}

		public List<Relativity.Client.DTOs.Choice> GetChoicesOnField(int fieldArtifactID)
		{
			return _query.GetChoicesOnField(fieldArtifactID);
		}
		public List<Relativity.Client.DTOs.Choice> GetChoicesOnField(Guid fieldGuid)
		{
			return _query.GetChoicesOnField(fieldGuid);
		}

		public List<FieldEntry> GetChoiceFields(int rdoTypeId)
		{
			var rdoCondition = new ObjectCondition
			{
				Field = Constants.Fields.ObjectTypeArtifactTypeId,
				Operator = ObjectConditionEnum.AnyOfThese,
				Value = new List<int> { rdoTypeId }
			};

			var choiceCondition = new TextCondition
			{
				Field = Constants.Fields.FieldType,
				Operator = TextConditionEnum.EqualTo,
				Value = FieldTypes.SingleChoice
			};

			var multiChoiceTextCondition = new TextCondition
			{
				Field = Constants.Fields.FieldType,
				Operator = TextConditionEnum.EqualTo,
				Value = Constants.Fields.MultipleChoice
			};

			Query query = new Query
			{
				ArtifactTypeName = "Field",
				Fields = new List<kCura.Relativity.Client.Field>(),
				Sorts = new List<Sort>()
				{
					new Sort()
					{
						Field = Constants.Fields.Name,
						Direction = SortEnum.Ascending
					}
				}
			};
			CompositeCondition documentLongTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, choiceCondition);
			CompositeCondition documentFixedLengthTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, multiChoiceTextCondition);
			query.Condition = new CompositeCondition(documentLongTextCondition, CompositeConditionEnum.Or, documentFixedLengthTextCondition);
			
			List<FieldEntry> fieldEntries = ConvertToFieldEntries(_query.GetChoicesByQuery(query));
			return fieldEntries;
		}

		public List<FieldEntry> ConvertToFieldEntries(List<kCura.Relativity.Client.Artifact> artifacts)
		{
			List<FieldEntry> fieldEntries = new List<FieldEntry>();

			foreach (kCura.Relativity.Client.Artifact artifact in artifacts)
			{
				foreach (kCura.Relativity.Client.Field field in artifact.Fields)
				{
					if (field.Name == Constants.Fields.Name)
					{
						fieldEntries.Add(new FieldEntry()
						{
							DisplayName = field.Value as string,
							FieldIdentifier = artifact.ArtifactID.ToString(),
							IsRequired = false
						});
						break;
					}
				}
			}
			return fieldEntries;
		}
	}
}
