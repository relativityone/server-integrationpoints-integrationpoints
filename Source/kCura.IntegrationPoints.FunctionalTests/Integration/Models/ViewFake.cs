using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.View;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class ViewFake : RdoFakeBase
    {
        public string Name { get; set; }

        public ViewFake() : base("View")
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

        public View ToView()
        {
            return new View()
            {
                ArtifactID = ArtifactId,
                Name = Name,
                Fields = Values.Values
                    .Select(x =>
                    {
                        FieldFake fieldFake = (FieldFake)x;
                        return new Relativity.Services.Field.FieldRef
                        {
                            Name = fieldFake.Name,
                            ArtifactID = fieldFake.ArtifactId
                        };
                    }).ToList()
            };
        }
    }
}
