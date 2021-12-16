using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
	public class FieldsMappingHelper : WorkspaceHelperBase
    {
        private const string FIXED_LENGTH_TEXT_NAME = "Fixed-Length Text";
        private const string LONG_TEXT_NAME = "Long Text";

		public FieldsMappingHelper(WorkspaceTest workspace) : base(workspace)
		{
		}
	
		public List<FieldMap> PrepareIdentifierFieldsMapping(WorkspaceTest destinationWorkspace)
		{
			FieldTest sourceControlNumber = Workspace.Fields.First(x => x.IsIdentifier);
			
			FieldTest destinationControlNumber = destinationWorkspace.Fields.First(x => x.IsIdentifier);

			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = sourceControlNumber.Name,
						FieldIdentifier = sourceControlNumber.ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					DestinationField = new FieldEntry
					{
						DisplayName = destinationControlNumber.Name,
						FieldIdentifier = destinationControlNumber.ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};
		}
		
		public List<FieldMap> PrepareIdentifierFieldsMappingForImport(string identifierFieldName)
		{
			FieldTest sourceControlNumber = Workspace.Fields.First(x => x.IsIdentifier);
			
			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = identifierFieldName,
						FieldIdentifier = identifierFieldName,
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					DestinationField = new FieldEntry
					{
						DisplayName = sourceControlNumber.Name,
						FieldIdentifier = sourceControlNumber.ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};
		}

		public List<FieldMap> PrepareIdentifierFieldsMappingForLoadFileImport(string identifierFieldName)
		{
			FieldTest sourceControlNumber = Workspace.Fields.First(x => x.IsIdentifier);

			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = identifierFieldName,
						FieldIdentifier = "0",
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					DestinationField = new FieldEntry
					{
						DisplayName = sourceControlNumber.Name,
						FieldIdentifier = sourceControlNumber.ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};
		}

		public List<FieldMap> PrepareIdentifierFieldsMappingForLDAPEntityImport()
		{
			Dictionary<string, FieldTest> entityFields = Workspace.Fields.Where(x => x.ObjectTypeId == Const.LDAP._ENTITY_TYPE_ARTIFACT_ID)
				.ToDictionary(x => x.Name, x => x);

			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "uid",
						FieldIdentifier = "uid",
						FieldType = FieldType.String,
						IsIdentifier = false,
						IsRequired = false,
						Type = null
					},
					DestinationField = new FieldEntry
					{
						DisplayName = entityFields["Unique ID"].Name,
						FieldIdentifier = entityFields["Unique ID"].ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = FIXED_LENGTH_TEXT_NAME
					},
					FieldMapType = FieldMapTypeEnum.Identifier
				},
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "sn",
						FieldIdentifier = "sn",
						FieldType = FieldType.String,
						IsIdentifier = false,
						IsRequired = false,
						Type = null
					},
					DestinationField = new FieldEntry
					{
						DisplayName = entityFields["Last Name"].Name,
						FieldIdentifier = entityFields["Last Name"].ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = false,
						IsRequired = false,
						Type = FIXED_LENGTH_TEXT_NAME
					},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "givenname",
						FieldIdentifier = "givenname",
						FieldType = FieldType.String,
						IsIdentifier = false,
						IsRequired = false,
						Type = null
					},
					DestinationField = new FieldEntry
					{
						DisplayName = entityFields["First Name"].Name,
						FieldIdentifier = entityFields["First Name"].ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = false,
						IsRequired = false,
						Type = FIXED_LENGTH_TEXT_NAME
					},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "manager",
						FieldIdentifier = "manager",
						FieldType = FieldType.String,
						IsIdentifier = false,
						IsRequired = false,
						Type = null
					},
					DestinationField = new FieldEntry
					{
						DisplayName = entityFields["Manager"].Name,
						FieldIdentifier = entityFields["Manager"].ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = false,
						IsRequired = false,
						Type = "Single Object"
					},
					FieldMapType = FieldMapTypeEnum.None
				}
			};
		}

		public List<FieldMap> PrepareLongTextFieldsMapping()
		{
            return AddFieldEntriesToFieldsMap(Const.LONG_TEXT_TYPE_ARTIFACT_ID, LONG_TEXT_NAME);
        }

        public List<FieldMap> PrepareFixedLengthTextFieldsMapping()
        {
            return AddFieldEntriesToFieldsMap(Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID, FIXED_LENGTH_TEXT_NAME);
        }

        private List<FieldMap> AddFieldEntriesToFieldsMap(int objectTypeId, string fieldType)
        {
            Dictionary<string, FieldTest> fields = Workspace.Fields.Where(x => x.ObjectTypeId == objectTypeId)
                .ToDictionary(x => x.Name, x => x);

            List<FieldMap> fieldsMap = new List<FieldMap>();

            foreach (var field in fields)
            {
                fieldsMap.Add(new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        DisplayName = field.Value.Name,
                        FieldIdentifier = field.Value.ArtifactId.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = false,
                        IsRequired = false,
                        Type = null,
                    },
                    DestinationField = new FieldEntry()
                    {
                        DisplayName = field.Value.Name,
                        FieldIdentifier = field.Value.ArtifactId.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = false,
                        IsRequired = true,
                        Type = fieldType
                    },
                    FieldMapType = FieldMapTypeEnum.None
                }
                );
            }

            return fieldsMap;
        }
	}


}
