using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class SourceProviderTest : RdoTestBase
    {
        public string Name { get; set; }

        public string Identifier { get; set; }

        public string SourceConfigurationUrl { get; set; }

        public string ApplicationIdentifier { get; set; }

        public string ViewConfigurationUrl { get; set; }

        public string Configuration { get; set; }

        public SourceProviderTest() : base("SourceProvider")
        {
        }

        public override List<Guid> Guids => new List<Guid>();

        public override RelativityObject ToRelativityObject()
        {
            return new RelativityObject()
            {
                ArtifactID = ArtifactId,
                Guids = new List<Guid>()
                {
                    new Guid("5be4a1f7-87a8-4cbe-a53f-5027d4f70b80")
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
                                new Guid("d0ecc6c9-472c-4296-83e1-0906f0c0fbb9")
                            }
                        },
                        Value = Identifier
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Source Configuration Url",
                            Guids = new List<Guid>()
                            {
                                new Guid("b1b34def-3e77-48c3-97d4-eae7b5ee2213")
                            }
                        },
                        Value = SourceConfigurationUrl
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Application Identifier",
                            Guids = new List<Guid>()
                            {
                                new Guid("0e696f9e-0e14-40f9-8cd7-34195defe5de")
                            }
                        },
                        Value = ApplicationIdentifier
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "View Configuration Url",
                            Guids = new List<Guid>()
                            {
                                new Guid("bb036af8-1309-4f66-98f3-3495285b4a4b")
                            }
                        },
                        Value = ViewConfigurationUrl
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Configuration",
                            Guids = new List<Guid>()
                            {
                                new Guid("a85e3e30-e56a-4ddb-9282-fc37dc5e70d3")
                            }
                        },
                        Value = Configuration
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Name",
                            Guids = new List<Guid>()
                            {
                                new Guid("9073997b-319e-482f-92fe-67e0b5860c1b")
                            }
                        },
                        Value = Name
                    },
                }
            };
        }

        public SourceProvider ToRdo()
        {
            return new SourceProvider
            {
                RelativityObject = ToRelativityObject(),
                ArtifactId = ArtifactId,
                ParentArtifactId = ParentObjectArtifactId,
                Config = null,
                Identifier = Identifier,
                SourceConfigurationUrl = SourceConfigurationUrl,
                ApplicationIdentifier = ApplicationIdentifier,
                ViewConfigurationUrl = ViewConfigurationUrl,
                Configuration = Configuration,
                Name = Name
            };
        }
    }
}
