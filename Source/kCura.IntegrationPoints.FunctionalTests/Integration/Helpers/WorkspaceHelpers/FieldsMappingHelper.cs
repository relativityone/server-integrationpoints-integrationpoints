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

        public List<FieldMap> PrepareIdentifierFieldsMapping(WorkspaceTest destinationWorkspace, int artifactTypeId)
        {
            FieldTest sourceIdentifier = Workspace.Fields.First(x => x.ObjectTypeId == artifactTypeId && x.IsIdentifier);

            FieldTest destinationIdentifier = destinationWorkspace.Fields.First(x => x.ObjectTypeId == artifactTypeId && x.IsIdentifier);

            return new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        DisplayName = sourceIdentifier.Name,
                        FieldIdentifier = sourceIdentifier.ArtifactId.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = true,
                        IsRequired = true,
                        Type = ""
                    },
                    DestinationField = new FieldEntry
                    {
                        DisplayName = destinationIdentifier.Name,
                        FieldIdentifier = destinationIdentifier.ArtifactId.ToString(),
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
            FieldTest sourceIdentifier = Workspace.Fields.First(x => x.IsIdentifier);

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
                        DisplayName = sourceIdentifier.Name,
                        FieldIdentifier = sourceIdentifier.ArtifactId.ToString(),
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
            FieldTest sourceIdentifier = Workspace.Fields.First(x => x.IsIdentifier);

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
                        DisplayName = sourceIdentifier.Name,
                        FieldIdentifier = sourceIdentifier.ArtifactId.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = true,
                        IsRequired = true,
                        Type = ""
                    },
                    FieldMapType = FieldMapTypeEnum.Identifier
                }
            };
        }

        public List<FieldMap> PrepareIdentifierOnlyFieldsMappingForLDAPEntityImport()
        {
            int _artifactTypeIdEntity = GetArtifactTypeIdByName(Const.Entity._ENTITY_OBJECT_NAME);
            Dictionary<string, FieldTest> destinationWorkspaceFields = Workspace.Fields.Where(x => x.ObjectTypeId == _artifactTypeIdEntity)
                .ToDictionary(x => x.Name, x => x);

            return new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = PrepareFieldEntry("uid", false, null),
                    DestinationField = PrepareFieldEntry("Unique ID", true, FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.Identifier
                }
            };
        }

        public List<FieldMap> PrepareIdentifierAndFirstAndLastNameFieldsMappingForEntitySync(WorkspaceTest destinationWorkspace)
        {
            int artifactTypeIdEntity = GetArtifactTypeIdByName(Const.Entity._ENTITY_OBJECT_NAME);
            Dictionary<string, FieldTest> sourceWorkspaceFields = Workspace.Fields.Where(x => x.ObjectTypeId == artifactTypeIdEntity)
                .ToDictionary(x => x.Name, x => x);
            Dictionary<string, FieldTest> destinationWorkspaceFields = destinationWorkspace.Fields.Where(x => x.ObjectTypeId == artifactTypeIdEntity)
                .ToDictionary(x => x.Name, x => x);
            return CreateEntitiesFieldMap(sourceWorkspaceFields, destinationWorkspaceFields);
        }

        public List<FieldMap> PrepareIdentifierAndFirstAndLastNameFieldsMappingForEntitySync()
        {
            int artifactTypeIdEntity = GetArtifactTypeIdByName(Const.Entity._ENTITY_OBJECT_NAME);
            Dictionary<string, FieldTest> destinationWorkspaceFields = Workspace.Fields.Where(x => x.ObjectTypeId == artifactTypeIdEntity)
                .ToDictionary(x => x.Name, x => x);
            return CreateEntitiesFieldMap(destinationWorkspaceFields);
        }

        public List<FieldMap> PrepareIdentifierAndFirstAndLastNameFieldsMappingForLDAPEntityImport()
        {
            int artifactTypeIdEntity = GetArtifactTypeIdByName(Const.Entity._ENTITY_OBJECT_NAME);
            Dictionary<string, FieldTest> destinationWorkspaceFields = Workspace.Fields.Where(x => x.ObjectTypeId == artifactTypeIdEntity)
                .ToDictionary(x => x.Name, x => x);
            List<FieldMap> IdentifierOnlyFieldMap = PrepareIdentifierOnlyFieldsMappingForLDAPEntityImport();
            List<FieldMap> WithoutIdentifierFieldMap = new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = PrepareFieldEntry("sn", false, null),
                    DestinationField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_LAST_NAME, false,
                        FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                },
                new FieldMap
                {
                    SourceField = PrepareFieldEntry("givenname", false, null),
                    DestinationField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_FIRST_NAME, false,
                        FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                },
                new FieldMap
                {
                    SourceField = PrepareFieldEntry("manager", false, null),
                    DestinationField = PrepareFieldEntry("Manager", false, "Single Object", destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                }
            };
            IdentifierOnlyFieldMap.AddRange(WithoutIdentifierFieldMap);
            List<FieldMap> joinedList = IdentifierOnlyFieldMap;
            return joinedList;
        }

        public List<FieldMap> PrepareLongTextFieldsMapping()
        {
            return AddFieldEntriesToFieldsMap(Const.LONG_TEXT_TYPE_ARTIFACT_ID, LONG_TEXT_NAME);
        }

        public List<FieldMap> PrepareFixedLengthTextFieldsMapping()
        {
            return AddFieldEntriesToFieldsMap(Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID, FIXED_LENGTH_TEXT_NAME);
        }

        private List<FieldMap> CreateEntitiesFieldMap(Dictionary<string, FieldTest> sourceWorkspaceFields, Dictionary<string, FieldTest> destinationWorkspaceFields)
        {
            return new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = PrepareFieldEntry("Unique ID", true, FIXED_LENGTH_TEXT_NAME, sourceWorkspaceFields),
                    DestinationField = PrepareFieldEntry("Unique ID", true, FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.Identifier
                },
                new FieldMap
                {
                    SourceField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_LAST_NAME, false, FIXED_LENGTH_TEXT_NAME, sourceWorkspaceFields),
                    DestinationField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_LAST_NAME, false, FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                },
                new FieldMap
                {
                    SourceField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_FIRST_NAME, false, FIXED_LENGTH_TEXT_NAME, sourceWorkspaceFields),
                    DestinationField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_FIRST_NAME, false, FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                },
                new FieldMap
                {
                    SourceField = PrepareFieldEntry("Manager", false, "Single Object", sourceWorkspaceFields),
                    DestinationField = PrepareFieldEntry("Manager", false, "Single Object", destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                }
            };
        }

        private List<FieldMap> CreateEntitiesFieldMap(Dictionary<string, FieldTest> destinationWorkspaceFields)
        {
            return new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = PrepareFieldEntry("Unique ID", true, FIXED_LENGTH_TEXT_NAME, null, true),
                    DestinationField = PrepareFieldEntry("Unique ID", true, FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields, true),
                    FieldMapType = FieldMapTypeEnum.Identifier
                },
                new FieldMap
                {
                    SourceField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_LAST_NAME, false, FIXED_LENGTH_TEXT_NAME, null, true),
                    DestinationField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_LAST_NAME, false, FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                },
                new FieldMap
                {
                    SourceField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_FIRST_NAME, false, FIXED_LENGTH_TEXT_NAME, null, true),
                    DestinationField = PrepareFieldEntry(Const.Entity._ENTITY_OBJECT_FIRST_NAME, false, FIXED_LENGTH_TEXT_NAME, destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                },
                new FieldMap
                {
                    SourceField = PrepareFieldEntry("Manager", false, "Single Object", null, true),
                    DestinationField = PrepareFieldEntry("Manager", false, "Single Object", destinationWorkspaceFields),
                    FieldMapType = FieldMapTypeEnum.None
                }
            };
        }

        private FieldEntry PrepareFieldEntry(string displayName, bool isIdentifier, string type, Dictionary<string, FieldTest> workspaceFields = null, bool trimDisplayName = false)
        {
            string name = trimDisplayName ? string.Concat(displayName.Where(x => !char.IsWhiteSpace(x))) : displayName;
            return new FieldEntry
            {
                DisplayName = workspaceFields == null ? name : workspaceFields[displayName].Name,
                FieldIdentifier = workspaceFields == null ? name : workspaceFields[displayName].ArtifactId.ToString(),
                FieldType = FieldType.String,
                IsIdentifier = isIdentifier,
                IsRequired = isIdentifier,
                Type = type
            };
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

        private int GetArtifactTypeIdByName(string name)
        {
            return Workspace.ObjectTypes.First(x => x.Name == name).ArtifactTypeId;
        }
    }

}
