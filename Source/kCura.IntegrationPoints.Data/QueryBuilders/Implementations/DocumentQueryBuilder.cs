using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class DocumentQueryBuilder : QueryBuilder
	{
		public DocumentQueryBuilder AddFolderCondition(int folderId, int viewId, bool includeSubFoldersTotals)
		{
			var folderNameCondition = new ObjectCondition("Folder Name", GetConditionOperator(includeSubFoldersTotals), folderId);
			Conditions.Add(folderNameCondition);

			var viewCondition = new ViewCondition(viewId);
			Conditions.Add(viewCondition);

			return this;
		}

		public DocumentQueryBuilder AddSavedSearchCondition(int savedSearchId)
		{
			var savedSearchCondition = new SavedSearchCondition(savedSearchId);
			Conditions.Add(savedSearchCondition);

			return this;
		}

		public DocumentQueryBuilder AddHasNativeCondition()
		{
			var hasNativeCondition = new BooleanCondition(DocumentFieldsConstants.HasNativeFieldGuid, BooleanConditionEnum.EqualTo, true);
			Conditions.Add(hasNativeCondition);

			return this;
		}

		public DocumentQueryBuilder AddHasImagesCondition()
		{
			var hasImagesCondition = new SingleChoiceCondition(DocumentFieldsConstants.HasImagesFieldGuid, SingleChoiceConditionEnum.AnyOfThese,
				new[] {DocumentFieldsConstants.HAS_IMAGES_YES_ARTIFACT_ID});
			Conditions.Add(hasImagesCondition);

			return this;
		}

		public DocumentQueryBuilder AllFields()
		{
			Fields = FieldValue.AllFields;

			return this;
		}

		public DocumentQueryBuilder NoFields()
		{
			Fields = FieldValue.NoFields;

			return this;
		}

		public DocumentQueryBuilder AddField(Guid fieldGuid)
		{
			Fields.Add(new FieldValue(fieldGuid));

			return this;
		}

		public DocumentQueryBuilder AddFields(List<Guid> fieldGuids)
		{
			Fields.AddRange(fieldGuids.Select(x => new FieldValue(x)));

			return this;
		}

		public override Query<RDO> Build()
		{
			return new Query<RDO>
			{
				ArtifactTypeID = (int)ArtifactType.Document,
				Fields = Fields,
				Condition = BuildCondition()
			};
		}

		private ObjectConditionEnum GetConditionOperator(bool includeChildren)
		{
			return includeChildren ? ObjectConditionEnum.AnyOfThese : ObjectConditionEnum.EqualTo;
		}
	}
}