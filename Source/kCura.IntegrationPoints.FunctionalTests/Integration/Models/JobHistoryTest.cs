using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class JobHistoryTest : RdoTestBase
    {
        public override List<Guid> Guids => Const.RdoGuids.JobHistory.Guids;

        public List<int> ItemsTransferredHistory { get; } = new List<int>();

        public List<int> ItemsWithErrorsHistory { get; } = new List<int>();

        public JobHistoryTest() : base("JobHistory")
        {
            Name = $"Job History";
        }

        protected override void SetField(Guid guid, object value)
        {
            if (guid == JobHistoryFieldGuids.ItemsWithErrorsGuid && value != null)
            {
                ItemsWithErrorsHistory.Add((int)value);
            }

            if (guid == JobHistoryFieldGuids.ItemsTransferredGuid && value != null)
            {
                ItemsTransferredHistory.Add((int)value);
            }

            base.SetField(guid, value);
        }

        public int[] IntegrationPoint
        {
            get => GetField(JobHistoryFieldGuids.IntegrationPointGuid) as int[];
            set => SetField(JobHistoryFieldGuids.IntegrationPointGuid, value);
        }

        public ChoiceRef JobStatus
        {
            get => GetField(JobHistoryFieldGuids.JobStatusGuid) as ChoiceRef;
            set => SetField(JobHistoryFieldGuids.JobStatusGuid, value);
        }

        public int? ItemsTransferred
        {
            get => GetField(JobHistoryFieldGuids.ItemsTransferredGuid) as int?;
            set => SetField(JobHistoryFieldGuids.ItemsTransferredGuid, value);
        }

        public int? ItemsWithErrors
        {
            get => GetField(JobHistoryFieldGuids.ItemsWithErrorsGuid) as int?;
            set => SetField(JobHistoryFieldGuids.ItemsWithErrorsGuid, value);
        }

        public DateTime? StartTimeUTC
        {
            get => GetField(JobHistoryFieldGuids.StartTimeUTCGuid) as DateTime?;
            set => SetField(JobHistoryFieldGuids.StartTimeUTCGuid, value);
        }

        public DateTime? EndTimeUTC
        {
            get => GetField(JobHistoryFieldGuids.EndTimeUTCGuid) as DateTime?;
            set => SetField(JobHistoryFieldGuids.EndTimeUTCGuid, value);
        }

        public string BatchInstance
        {
            get => GetField(JobHistoryFieldGuids.BatchInstanceGuid) as string;
            set => SetField(JobHistoryFieldGuids.BatchInstanceGuid, value);
        }

        public string DestinationWorkspace
        {
            get => GetField(JobHistoryFieldGuids.DestinationWorkspaceGuid) as string;
            set => SetField(JobHistoryFieldGuids.DestinationWorkspaceGuid, value);
        }

        public long? TotalItems
        {
            get => GetField(JobHistoryFieldGuids.TotalItemsGuid) as long?;
            set => SetField(JobHistoryFieldGuids.TotalItemsGuid, value);
        }

        public int[] DestinationWorkspaceInformation
        {
            get => GetField(JobHistoryFieldGuids.DestinationWorkspaceInformationGuid) as int[];
            set => SetField(JobHistoryFieldGuids.DestinationWorkspaceInformationGuid, value);
        }

        public ChoiceRef JobType
        {
            get => GetField(JobHistoryFieldGuids.JobTypeGuid) as ChoiceRef;
            set => SetField(JobHistoryFieldGuids.JobTypeGuid, value);
        }

        public string DestinationInstance
        {
            get => GetField(JobHistoryFieldGuids.DestinationInstanceGuid) as string;
            set => SetField(JobHistoryFieldGuids.DestinationInstanceGuid, value);
        }

        public string FilesSize
        {
            get => GetField(JobHistoryFieldGuids.FilesSizeGuid) as string;
            set => SetField(JobHistoryFieldGuids.FilesSizeGuid, value);
        }

        public string Overwrite
        {
            get => GetField(JobHistoryFieldGuids.OverwriteGuid) as string;
            set => SetField(JobHistoryFieldGuids.OverwriteGuid, value);
        }

        public string JobID
        {
            get => GetField(JobHistoryFieldGuids.JobIDGuid) as string;
            set => SetField(JobHistoryFieldGuids.JobIDGuid, value);
        }

        public string Name
        {
            get => GetField(JobHistoryFieldGuids.NameGuid) as string;
            set => SetField(JobHistoryFieldGuids.NameGuid, value);
        }

        public override RelativityObject ToRelativityObject()
        {
            return new RelativityObject()
            {
                ArtifactID = ArtifactId,
                Guids = new List<Guid>()
                {
                    ObjectTypeGuids.JobHistoryGuid
                },
                Name = Name,
                FieldValues = new List<FieldValuePair>()
                {
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.IntegrationPoint,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.IntegrationPointGuid
                            }
                        },
                        Value = IntegrationPoint
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.JobStatus,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.JobStatusGuid
                            }
                        },
                        Value = JobStatus
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.ItemsTransferred,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.ItemsTransferredGuid
                            }
                        },
                        Value = ItemsTransferred
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.ItemsWithErrors,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.ItemsWithErrorsGuid
                            }
                        },
                        Value = ItemsWithErrors
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.StartTimeUTC,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.StartTimeUTCGuid
                            }
                        },
                        Value = StartTimeUTC
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.EndTimeUTC,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.EndTimeUTCGuid
                            }
                        },
                        Value = EndTimeUTC
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.BatchInstance,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.BatchInstanceGuid
                            }
                        },
                        Value = BatchInstance
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.DestinationWorkspace,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.DestinationWorkspaceGuid
                            }
                        },
                        Value = DestinationWorkspace
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.TotalItems,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.TotalItemsGuid
                            }
                        },
                        Value = TotalItems
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.DestinationWorkspaceInformation,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.DestinationWorkspaceInformationGuid
                            }
                        },
                        Value = DestinationWorkspaceInformation
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.JobType,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.JobTypeGuid
                            }
                        },
                        Value = JobType
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.DestinationInstance,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.DestinationInstanceGuid
                            }
                        },
                        Value = DestinationInstance
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.FilesSize,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.FilesSizeGuid
                            }
                        },
                        Value = FilesSize
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.Overwrite,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.OverwriteGuid
                            }
                        },
                        Value = Overwrite
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.JobID,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.JobIDGuid
                            }
                        },
                        Value = JobID
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = JobHistoryFields.Name,
                            Guids = new List<Guid>()
                            {
                                JobHistoryFieldGuids.NameGuid
                            }
                        },
                        Value = Name
                    },
                }
            };
        }
    }
}
