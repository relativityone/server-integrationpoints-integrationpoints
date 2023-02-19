using System;
using System.Collections.Generic;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class SourceWorkspaceDTO
    {
        public static readonly Guid ObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");

        public int ArtifactTypeId { get; set; }

        public int ArtifactId { get; set; }

        public string Name { get; set; }

        public int SourceCaseArtifactId { get; set; }

        public string SourceCaseName { get; set; }

        public string SourceInstanceName { get; set; }

        public SourceWorkspaceDTO()
        {
        }

        public SourceWorkspaceDTO(int artifactId, List<FieldValuePair> fieldValues) : this(fieldValues)
        {
            ArtifactId = artifactId;
        }

        public SourceWorkspaceDTO(List<FieldValuePair> fieldValues)
        {
            foreach (FieldValuePair fieldValue in fieldValues)
            {
                if (fieldValue.Field.Name == Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME && fieldValue.Value != null)
                {
                    SourceCaseArtifactId = Convert.ToInt32(fieldValue.Value);
                }
                else if (fieldValue.Field.Name == Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME)
                {
                    SourceCaseName = fieldValue.Value as string;
                }
                else if (fieldValue.Field.Name == Constants.SOURCEWORKSPACE_NAME_FIELD_NAME)
                {
                    Name = fieldValue.Value as string;
                }
                else if (fieldValue.Field.Name == Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME)
                {
                    SourceInstanceName = fieldValue.Value as string;
                }
            }
        }

        public List<FieldRefValuePair> FieldRefValuePairs => CreateFieldRefValuePairs();

        public ObjectTypeRef ObjectTypeRef => new ObjectTypeRef
        {
            Guid = ObjectTypeGuid
        };

        private List<FieldRefValuePair> CreateFieldRefValuePairs()
        {
            return new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Name = Constants.SOURCEWORKSPACE_NAME_FIELD_NAME },
                    Value = Name
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Name = Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME },
                    Value = SourceCaseArtifactId
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Name = Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME },
                    Value = SourceCaseName
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Name = Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME },
                    Value = SourceInstanceName
                }
            };
        }

        public static class Fields
        {
            public static readonly Guid CaseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
            public static readonly Guid CaseNameFieldNameGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
            public static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
            public static readonly Guid SourceWorkspaceFieldOnDocumentGuid = new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f");

            public static IDictionary<Guid, BaseFieldRequest> GetFieldsDefinition(int objectTypeDescriptorArtifactTypeId)
            {
                var objectType = new ObjectTypeIdentifier()
                {
                    ArtifactTypeID = objectTypeDescriptorArtifactTypeId
                };
                return new Dictionary<Guid, BaseFieldRequest>
                {
                    {
                        CaseIdFieldNameGuid, new WholeNumberFieldRequest
                        {
                            Name = Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME,
                            ObjectType = objectType,
                            IsRequired = true,
                            IsLinked = false,
                            OpenToAssociations = false,
                            AllowSortTally = false,
                            AllowGroupBy = false,
                            AllowPivot = false,
                            Width = 100,
                            Wrapping = false
                        }
                    },
                    {
                        CaseNameFieldNameGuid, new FixedLengthFieldRequest
                        {
                            Name = Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME,
                            ObjectType = objectType,
                            IsRequired = true,
                            IncludeInTextIndex = false,
                            IsLinked = false,
                            AllowHtml = false,
                            AllowSortTally = false,
                            AllowGroupBy = false,
                            AllowPivot = false,
                            OpenToAssociations = false,
                            Width = 100,
                            Wrapping = false,
                            HasUnicode = false,
                            Length = 255
                        }
                    },
                    {
                        InstanceNameFieldGuid, new FixedLengthFieldRequest
                        {
                            Name = Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME,
                            ObjectType = objectType,
                            IsRequired = true,
                            IncludeInTextIndex = false,
                            IsLinked = false,
                            AllowHtml = false,
                            AllowSortTally = false,
                            AllowGroupBy = false,
                            AllowPivot = false,
                            OpenToAssociations = false,
                            Width = 100,
                            Wrapping = false,
                            HasUnicode = false,
                            Length = 255
                        }
                    }
                };
            }
        }
    }
}
