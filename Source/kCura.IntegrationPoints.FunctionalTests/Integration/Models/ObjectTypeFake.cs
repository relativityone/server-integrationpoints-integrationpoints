using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class ObjectTypeFake : RdoFakeBase
    {
        public string Name { get; set; }

        public string ObjectType { get; set; }

        public int ObjectTypeArtifactTypeId { get; set; }

        public int ArtifactTypeId { get; set; }

        public Guid Guid { get; set; }

        public ObjectTypeFake() : base("Object Type")
        {
        }

        public override List<Guid> Guids => new List<Guid>();

        public override RelativityObject ToRelativityObject()
        {
            return new RelativityObject
            {
                ArtifactID = ArtifactId,
                Guids = new List<Guid> { Guid },
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = ParentObjectArtifactId
                },
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair
                    {
                        Field = new Field
                        {
                            Name = "Name"
                        },
                        Value = Name
                    }
                }
            };
        }
    }
}
