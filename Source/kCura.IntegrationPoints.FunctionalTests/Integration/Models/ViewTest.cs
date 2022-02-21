using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.View;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class ViewTest : RdoTestBase
    {
        public string Name { get; set; }

        public ViewTest() : base("View")
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
                    ArtifactID = ParenObjectArtifactId
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
                        FieldTest fieldTest = (FieldTest)x;
                        return new Relativity.Services.Field.FieldRef
                        {
                            Name = fieldTest.Name,
                            ArtifactID = fieldTest.ArtifactId
                        };
                    }).ToList()
            };
        }
    }
}
