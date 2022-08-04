using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class FieldTest : RdoTestBase
    {
        public string Name { get; set; }

        public int ObjectTypeId { get; set; }

        public bool IsIdentifier { get; set; }

        public Guid Guid { get; set; }

        public FieldTest() : base("Field")
        {
        }

        public FieldTest(int artifactId) : base("Field", artifactId)
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
                            Name = "Is Identifier"
                        },
                        Value = IsIdentifier
                    },
                    new FieldValuePair
                    {
                        Field = new Field
                        {
                            Name = "Name"
                        },
                        Value = Name
                    }
                },
            };
        }
    }
}
