using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using Fields = kCura.IntegrationPoints.Core.Constants.Fields;

namespace kCura.IntegrationPoints.Core.Services
{
    public class FieldService : IFieldService
    {
		private const string _GET_TEXT_FIELDS_ERROR = "Integration Points failed during getting text fields";

		private readonly IChoiceService _choiceService;
		private readonly IRSAPIClient _client;

	    public FieldService(IChoiceService choiceService, IRSAPIClient client)
	    {
			_choiceService = choiceService;
		    _client = client;
	    }

		public List<FieldEntry> GetTextFields(int rdoTypeId, bool longTextFieldsOnly)
		{
			var rdoCondition = new ObjectCondition
			{
				Field = Fields.ObjectTypeArtifactTypeId,
				Operator = ObjectConditionEnum.AnyOfThese,
				Value = new List<int> { rdoTypeId }
			};

			var longTextCondition = new TextCondition
			{
				Field = Fields.FieldType,
				Operator = TextConditionEnum.EqualTo,
				Value = FieldTypes.LongText
			};

			var fixedLengthTextCondition = new TextCondition
			{
				Field = Fields.FieldType,
				Operator = TextConditionEnum.EqualTo,
				Value = FieldTypes.FixedLengthText
			};

			var query = new Query
			{
				ArtifactTypeName = "Field",
				Fields = new List<Field>(),
				Sorts = new List<Sort>()
				{
					new Sort
					{
						Field = Fields.Name,
						Direction = SortEnum.Ascending
					}
				}
			};
			var documentLongTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, longTextCondition);
			var documentFixedLengthTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, fixedLengthTextCondition);
			query.Condition = longTextFieldsOnly ? documentLongTextCondition : new CompositeCondition(documentLongTextCondition, CompositeConditionEnum.Or, documentFixedLengthTextCondition);

			QueryResult result;
			try
			{
				result = _client.Query(_client.APIOptions, query);
				if (!result.Success)
				{
					throw new IntegrationPointsException(result.Message);
				}
			}
			catch (IntegrationPointsException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new IntegrationPointsException(_GET_TEXT_FIELDS_ERROR, ex);
			}

			List<FieldEntry> fieldEntries = _choiceService.ConvertToFieldEntries(result.QueryArtifacts);
			return fieldEntries;
		}

    }
}
