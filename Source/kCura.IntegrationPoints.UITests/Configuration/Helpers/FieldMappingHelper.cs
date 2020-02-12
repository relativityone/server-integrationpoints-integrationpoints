using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity;
using Relativity.Services.Objects.DataContracts;


namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class FieldMappingHelper
	{
		private const int _BATCH_SIZE = 50;

		private readonly IReadOnlyList<string> _fieldMapWhiteList = new List<string>
		{
			"Extracted Text",
			"Production",
			"Production Errors",
			"Relativity Native Time Zone Offset"
		};

		private readonly IReadOnlyList<string> _fieldMapBlackList = new List<string>
		{
			"FileIcon",
			//RIP fields
			"Relativity Source Case",
			"Relativity Source Job",
			"Relativity Destination Case",
			"Job History"
		};

		private readonly TestContext _testContext;


		public FieldMappingHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		private async Task<List<FieldObject>> RetrieveAllDocumentsFieldsFromWorkspaceAsync()
		{
			QueryRequest fieldsRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.Field},
				Condition = "'Object Type Artifact Type ID' == 10 ",
				Fields = new[]
				{
					new FieldRef {Name = "Name"},
					new FieldRef {Name = "Field Type"},
					new FieldRef {Name = "Keywords"},
					new FieldRef {Name = "Is Identifier"}
				},
			};

			int totalCount = 0;
			var fieldsFromWorkspace = new List<FieldObject>();
			do
			{
				ResultSet<RelativityObject> fieldsMapped =
					await _testContext.ObjectManager.QueryAsync(fieldsRequest, fieldsFromWorkspace.Count,
						_BATCH_SIZE).ConfigureAwait(false);
				fieldsFromWorkspace.AddRange(fieldsMapped.Items.Select(i => new FieldObject()
				{
					ArtifactID = i.ArtifactID,
					Name = i.FieldValues.First(fv => fv.Field.Name == "Name").Value.ToString(),
					Type = i.FieldValues.First(fv => fv.Field.Name == "Field Type").Value.ToString(),
					Keywords = i.FieldValues.First(fv => fv.Field.Name == "Keywords").Value.ToString(),
					IsIdentifier = (bool) i.FieldValues.First(fv => fv.Field.Name == "Is Identifier").Value
				}));
				totalCount = fieldsMapped.TotalCount;
			} while (fieldsFromWorkspace.Count < totalCount);

			return fieldsFromWorkspace;
		}

		private List<FieldObject> RemoveSystemFieldObjects(List<FieldObject> fieldsFromWorkspace)
		{
			fieldsFromWorkspace.RemoveAll(fo => fo.Keywords == "System" && !_fieldMapWhiteList.Contains(fo.Name));
			return fieldsFromWorkspace;
		}

		private List<FieldObject> RemoveOpenToAssociationFieldObjects(List<FieldObject> fieldsFromWorkspace)
		{
			fieldsFromWorkspace.RemoveAll(fo =>
				fo.Name.Contains("::") && !_fieldMapWhiteList.Contains(fo.Name));
			return fieldsFromWorkspace;
		}

		private List<FieldObject> RemoveBlackListedFieldObjects(List<FieldObject> fieldsFromWorkspace)
		{
			fieldsFromWorkspace.RemoveAll(fo => _fieldMapBlackList.Contains(fo.Name));
			return fieldsFromWorkspace;
		}

		public async Task<List<FieldObject>> RetrieveFilteredDocumentsFieldsFromWorkspaceAsync()
		{
			List<FieldObject> fieldsFromWorkspace =
				await RetrieveAllDocumentsFieldsFromWorkspaceAsync().ConfigureAwait(false);
			fieldsFromWorkspace = RemoveSystemFieldObjects(fieldsFromWorkspace);
			fieldsFromWorkspace = RemoveOpenToAssociationFieldObjects(fieldsFromWorkspace);
			fieldsFromWorkspace = RemoveBlackListedFieldObjects(fieldsFromWorkspace);
			return fieldsFromWorkspace;
		}
	}
}