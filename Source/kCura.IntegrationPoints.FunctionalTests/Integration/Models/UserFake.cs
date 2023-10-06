using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class UserFake : RdoFakeBase
    {
        public string Name { get; set; }

        public UserFake() : base("User")
        {
        }

        public override List<Guid> Guids => new List<Guid>();

        public override RelativityObject ToRelativityObject()
        {
            return new RelativityObject
            {
                ArtifactID = ArtifactId,
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
                            Name = "Name",
                        },
                        Value = Name
                    }
                },
            };
        }
    }
}
