using System;
using System.Collections.Generic;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class SourceJobDTO
    {
        public static readonly Guid ObjectTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

        public int SourceWorkspaceArtifactId { get; set; }

        public int ArtifactId { get; set; }

        public int ArtifactTypeId { get; set; }

        public string Name { get; set; }

        public int JobHistoryArtifactId { get; set; }

        public string JobHistoryName { get; set; }

        public List<FieldRefValuePair> FieldRefValuePairs => CreateFieldRefValuePairs();

        public ObjectTypeRef ObjectTypeRef => new ObjectTypeRef
        {
            Guid = ObjectTypeGuid
        };

        public RelativityObjectRef ParentObject => new RelativityObjectRef
        {
            ArtifactID = SourceWorkspaceArtifactId
        };

        private List<FieldRefValuePair> CreateFieldRefValuePairs()
        {
            return new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef {Name = Constants.SOURCEJOB_NAME_FIELD_NAME },
                    Value = Name
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef {Name = Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME },
                    Value = JobHistoryArtifactId
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef {Name = Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME },
                    Value = JobHistoryName
                }
            };
        }

        public static class Fields
        {
            public static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
            public static readonly Guid JobHistoryNameFieldGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
            public static readonly Guid JobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");

            public static IDictionary<Guid, BaseFieldRequest> GetFieldsDefinition(int objectTypeDescriptorArtifactId)
            {
                var objectType = new ObjectTypeIdentifier
                {
                    ArtifactTypeID = objectTypeDescriptorArtifactId
                };
                return new Dictionary<Guid, BaseFieldRequest>
                {
                    {
                        JobHistoryIdFieldGuid, new WholeNumberFieldRequest()
                        {
                            Name = Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME,
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
                        JobHistoryNameFieldGuid, new FixedLengthFieldRequest
                        {
                            Name = Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME,
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
