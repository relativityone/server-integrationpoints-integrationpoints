using kCura.IntegrationPoints.Core.Contracts.Entity;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class EntityTest : RdoTestBase
    {
        public override List<Guid> Guids => Const.RdoGuids.Entity.Guids;

        public string UniqueId
        {
            get => GetField(EntityFieldGuids.UniqueIdGuid) as string;
            set => SetField(EntityFieldGuids.UniqueIdGuid, value);
        }

        public string FirstName
        {
            get => GetField(EntityFieldGuids.FirstNameGuid) as string;
            set => SetField(EntityFieldGuids.FirstNameGuid, value);
        }

        public string LastName
        {
            get => GetField(EntityFieldGuids.LastNameGuid) as string;
            set => SetField(EntityFieldGuids.LastNameGuid, value);
        }

        public string FullName
        {
            get => GetField(EntityFieldGuids.FullNameGuid) as string;
            set => SetField(EntityFieldGuids.FullNameGuid, value);
        }

        public string Manager
        {
            get => GetField(EntityFieldGuids.ManagerGuid) as string;
            set => SetField(EntityFieldGuids.ManagerGuid, value);
        }

        public EntityTest() : base("Entity")
        {
        }

        public override RelativityObject ToRelativityObject()
        {
            return new RelativityObject()
            {
                ArtifactID = ArtifactId,
                Guids = new List<Guid>()
                {
                    ObjectTypeGuids.Entity
                },
                FieldValues = new List<FieldValuePair>()
                {
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = EntityFieldNames.UniqueId,
                            Guids = new List<Guid>()
                            {
                                EntityFieldGuids.UniqueIdGuid
                            }
                        },
                        Value = UniqueId
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = EntityFieldNames.FirstName,
                            Guids = new List<Guid>()
                            {
                                EntityFieldGuids.FirstNameGuid
                            }
                        },
                        Value = FirstName
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = EntityFieldNames.LastName,
                            Guids = new List<Guid>()
                            {
                                EntityFieldGuids.LastNameGuid
                            }
                        },
                        Value = LastName
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = EntityFieldNames.FullName,
                            Guids = new List<Guid>()
                            {
                                EntityFieldGuids.FullNameGuid
                            }
                        },
                        Value = FullName
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = EntityFieldNames.Manager,
                            Guids = new List<Guid>()
                            {
                                EntityFieldGuids.ManagerGuid
                            }
                        },
                        Value = Manager
                    },
                }
            };
        }
    }
}
