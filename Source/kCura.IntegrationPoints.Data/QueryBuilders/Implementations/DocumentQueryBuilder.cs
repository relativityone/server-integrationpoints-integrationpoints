using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class DocumentQueryBuilder : QueryBuilder
	{
		public DocumentQueryBuilder AddFolderCondition(int folderId, int viewId, bool includeSubFoldersTotals)
		{
			string folderNameCondition = includeSubFoldersTotals
				? $"'Folder Name' IN OBJECT [{folderId}]"
				: $"'Folder Name' == OBJECT {folderId}";

			Conditions.Add(folderNameCondition);

			string viewCondition = $"'ArtifactId' IN VIEW {viewId}";
			Conditions.Add(viewCondition);

			return this;
		}

		public DocumentQueryBuilder AddSavedSearchCondition(int savedSearchId)
		{
			string condition = $"'ArtifactId' IN SAVEDSEARCH {savedSearchId}";
			Conditions.Add(condition);

			return this;
		}

		public DocumentQueryBuilder AddHasNativeCondition()
		{
			string condition = $"'{DocumentFieldsConstants.HasNativeFieldGuid}' == true";
			Conditions.Add(condition);

			return this;
		}

		public DocumentQueryBuilder AddHasImagesCondition(int choiceArtifactId)
		{
			string condition = $"'{DocumentFieldsConstants.HasImagesFieldName}' == CHOICE {choiceArtifactId}";
			Conditions.Add(condition);

			return this;
		}

		public DocumentQueryBuilder NoFields()
		{
			Fields = new List<FieldRef>();
			return this;
		}

		public DocumentQueryBuilder AddField(Guid fieldGuid)
		{
			Fields.Add(new FieldRef { Guid = fieldGuid });

			return this;
		}

		public DocumentQueryBuilder AddFields(List<Guid> fieldGuids)
		{
			Fields.AddRange(fieldGuids.Select(x => new FieldRef { Guid = x }));

			return this;
		}

		public override QueryRequest Build()
		{
			return new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = Guid.Parse(ObjectTypeGuids.Document)
				},
				Fields = Fields,
				Condition = BuildCondition()
			};
		}
	}
}
