using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;
using Fields = kCura.IntegrationPoints.Core.Constants.Fields;
using Sort = Relativity.Services.Objects.DataContracts.Sort;
using SortEnum = Relativity.Services.Objects.DataContracts.SortEnum;

namespace kCura.IntegrationPoints.Core.Services
{
    public class FieldService : IFieldService
    {
		private const string _GET_TEXT_FIELDS_ERROR = "Integration Points failed during getting text fields";

	    private readonly IRelativityObjectManager _relativityObjectManager;

	    public FieldService(IRelativityObjectManager relativityObjectManager)
	    {
		    _relativityObjectManager = relativityObjectManager;
	    }

		public List<FieldEntry> GetTextFields(int rdoTypeId, bool longTextFieldsOnly)
		{
			string rdoCondition = $"'{Constants.Fields.ObjectTypeArtifactTypeId}' == OBJECT {rdoTypeId}";
			string longTextCondition = $"'{Fields.FieldType}' == '{FieldTypes.LongText}'";
			string fixedLengthTextCondition = $"'{Fields.FieldType}' == '{FieldTypes.FixedLengthText}'";

			string textFieldsCondition = longTextFieldsOnly
				? longTextCondition
				: $"{longTextCondition} OR {fixedLengthTextCondition}";

			string condition = $"{rdoCondition} AND ({textFieldsCondition})";

			var sorts = new List<Sort>
			{
				new Sort
				{
					Direction = SortEnum.Ascending,
					FieldIdentifier = new FieldRef {Name = Fields.Name}
				}
			};

			var query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Condition = condition,
				Sorts = sorts,
				Fields = new List<FieldRef> { new FieldRef { Name = Fields.Name } }
			};

			try
			{
				var results = _relativityObjectManager.Query(query);
				List<FieldEntry> fieldEntries = results
					.Select(f => new FieldEntry
					{
						DisplayName = GetFieldName(f),
						FieldIdentifier = f.ArtifactID.ToString(),
						IsRequired = false
					})
					.ToList();
				return fieldEntries;
			}
			catch (Exception ex)
			{
				// TODO add logging
				throw new IntegrationPointsException(_GET_TEXT_FIELDS_ERROR, ex);
			}
		}

		private string GetFieldName(RelativityObject obj)
		{
			return GetFieldValue(Fields.Name, obj) as string;
		}

		private object GetFieldValue(string fieldName, RelativityObject obj)
		{
			if (String.IsNullOrWhiteSpace(fieldName) || obj == null)
			{
				return null;
			}
			return obj.FieldValues?.FirstOrDefault(x => x.Field.Name == fieldName)?.Value;
		}

    }
}
