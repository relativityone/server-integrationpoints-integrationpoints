using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;
using GlobalConst = Relativity.IntegrationPoints.Tests.Common.GlobalConst;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class DestinationProviderFake : RdoFakeBase
    {
        public string Identifier { get; set; }

        public string ApplicationIdentifier { get; set; }

        public string Name { get; set; }

        public DestinationProviderFake() : base("DestinationProvider")
        {
            Name = $"Fake Destination Provider";
            Identifier = Guid.NewGuid().ToString();
            ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID;
        }

        public override List<Guid> Guids => new List<Guid>();

        public override RelativityObject ToRelativityObject()
        {
            return new RelativityObject()
            {
                ArtifactID = ArtifactId,
                Guids = new List<Guid>()
                {
                    new Guid("d014f00d-f2c0-4e7a-b335-84fcb6eae980")
                },
                Name = Name,
                FieldValues = new List<FieldValuePair>()
                {
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Identifier",
                            Guids = new List<Guid>()
                            {
                                new Guid("9fa104ac-13ea-4868-b716-17d6d786c77a")
                            }
                        },
                        Value = Identifier
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Application Identifier",
                            Guids = new List<Guid>()
                            {
                                new Guid("92892e25-0927-4073-b03d-e6a94ff80450")
                            }
                        },
                        Value = ApplicationIdentifier
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Name",
                            Guids = new List<Guid>()
                            {
                                new Guid("3ed18f54-c75a-4879-92a8-5ae23142bbeb")
                            }
                        },
                        Value = Name
                    }
                }
            };
        }

        public DestinationProvider ToRdo()
        {
            return new DestinationProvider
            {
                RelativityObject = ToRelativityObject(),
                ArtifactId = ArtifactId,
                ParentArtifactId = ParentObjectArtifactId,
                Identifier = Identifier,
                ApplicationIdentifier = ApplicationIdentifier,
                Name = Name
            };
        }
    }
}
