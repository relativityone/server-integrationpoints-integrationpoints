using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class JobHistoryErrorTest : RdoTestBase
	{
		public override List<Guid> Guids => Const.RdoGuids.JobHistoryError.Guids;

		public JobHistoryErrorTest() : base("JobHistoryError")
		{
		}

		public int? JobHistory
		{
			get => GetField(JobHistoryErrorFieldGuids.JobHistoryGuid) as int?;
			set => SetField(JobHistoryErrorFieldGuids.JobHistoryGuid, value);
		}

		public ChoiceRef ErrorType
		{
			get => GetField(JobHistoryErrorFieldGuids.ErrorTypeGuid) as ChoiceRef;
			set => SetField(JobHistoryErrorFieldGuids.ErrorTypeGuid, value);
		}

		public string Name
		{
			get => GetField(JobHistoryErrorFieldGuids.NameGuid) as string;
			set => SetField(JobHistoryErrorFieldGuids.NameGuid, value);
		}

		public override RelativityObject ToRelativityObject()
		{
			return new RelativityObject()
			{
				ArtifactID = ArtifactId,
				Guids = new List<Guid>()
				{
					ObjectTypeGuids.JobHistoryErrorGuid
				},
				Name = Name,
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = JobHistoryErrorFields.JobHistory,
							Guids = new List<Guid>()
							{
								JobHistoryErrorFieldGuids.JobHistoryGuid
							}
						},
						Value = JobHistory
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = JobHistoryErrorFields.ErrorType,
							Guids = new List<Guid>()
							{
								JobHistoryErrorFieldGuids.ErrorTypeGuid
							}
						},
						Value = ErrorType
					},
				}
			};
		}
	}
}
