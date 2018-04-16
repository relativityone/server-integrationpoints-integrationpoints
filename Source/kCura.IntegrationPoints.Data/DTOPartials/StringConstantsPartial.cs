using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data
{
	public partial class JobHistoryFields
	{
		public static IEnumerable<FieldRef> SlimFieldList { get; private set; } = new List<FieldRef>
		{
			new FieldRef {Name = "ArtifactId"},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.IntegrationPoint)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.JobStatus)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.ItemsTransferred)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.ItemsWithErrors)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.StartTimeUTC)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.EndTimeUTC)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.BatchInstance)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.DestinationWorkspace)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.TotalItems)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.DestinationWorkspaceInformation)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.JobType)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.DestinationInstance)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.FilesSize)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.Overwrite)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.JobID)},
			new FieldRef {Guid = new Guid(JobHistoryFieldGuids.Name)}
		};
	}
}
