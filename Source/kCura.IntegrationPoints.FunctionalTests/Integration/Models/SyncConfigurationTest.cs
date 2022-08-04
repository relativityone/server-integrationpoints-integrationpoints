using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class SyncConfigurationTest : RdoTestBase
    {
        public int JobHistoryId { get; set; }

        public bool Resuming { get; set; }

        public SyncConfigurationTest() : base("SyncConfiguration")
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
                    ObjectTypeGuids.SyncConfigurationGuid
                },
                FieldValues = new List<FieldValuePair>()
                {
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = Const.RdoGuids.SyncConfiguration.JobHistoryId,
                            Guids = new List<Guid>()
                            {
                                Const.RdoGuids.SyncConfiguration.JobHistoryIdGuid
                            }
                        },
                        Value = JobHistoryId
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = Const.RdoGuids.SyncConfiguration.Resuming,
                            Guids = new List<Guid>()
                            {
                                Const.RdoGuids.SyncConfiguration.ResumingGuid
                            }
                        },
                        Value = Resuming
                    }
                }
            };
        }
    }
}
