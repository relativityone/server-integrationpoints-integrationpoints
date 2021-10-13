using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class IntegrationPointTest : RdoTestBase
	{
		public static Guid FieldsMappingGuid { get; } = new Guid("1b065787-a6e4-4d70-a7ed-f49d770f0bc7");
		
		public DateTime? NextScheduledRuntimeUTC { get; set; }

		public DateTime? LastRuntimeUTC { get; set; }

		public string FieldMappings { get; set; } = "[]";

		public bool? EnableScheduler { get; set; }

		public string SourceConfiguration { get; set; } = "{}";

		public string DestinationConfiguration { get; set; } = "{}";

		public int? SourceProvider { get; set; }

		public string ScheduleRule { get; set; }

		public ChoiceRef OverwriteFields { get; set; } = OverwriteFieldsChoices.IntegrationPointAppendOnly;

		public int? DestinationProvider { get; set; }

		public int[] JobHistory { get; set; } = new int[0];

		public bool? LogErrors { get; set; }

		public string EmailNotificationRecipients { get; set; }

		public bool? HasErrors { get; set; }

		public int? Type { get; set; }

		public string SecuredConfiguration { get; set; }

		public bool? PromoteEligible { get; set; }

		public string Name { get; set; }

		public string SecuredConfigurationDecrypted { get; set; }

		public IntegrationPointTest() : base("IntegrationPoint")
		{
			Name = $"Integration Point (Artifact ID {ArtifactId})";
		}

		public override List<Guid> Guids => new List<Guid>();

		public override RelativityObject ToRelativityObject()
		{
			return new RelativityObject()
			{
				ArtifactID = ArtifactId,
				Name = Name,
				Guids = new List<Guid>()
				{
					new Guid("03d4f67e-22c9-488c-bee6-411f05c52e01")
				},
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Next Scheduled Runtime (UTC)",
							Guids = new List<Guid>()
							{
								new Guid("5b1c9986-f166-40e4-a0dd-a56f185ff30b")
							}
						},
						Value = NextScheduledRuntimeUTC
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Last Runtime (UTC)",
							Guids = new List<Guid>()
							{
								new Guid("90d58af1-f79f-40ae-85fc-7e42f84dbcc1")
							}
						},
						Value = LastRuntimeUTC
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Field Mappings",
							Guids = new List<Guid>()
							{
								FieldsMappingGuid
							}
						},
						Value = FieldMappings
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Enable Scheduler",
							Guids = new List<Guid>()
							{
								new Guid("bcdafc41-311e-4b66-8084-4a8e0f56ca00")
							}
						},
						Value = EnableScheduler
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Source Configuration",
							Guids = new List<Guid>()
							{
								new Guid("b5000e91-82bd-475a-86e9-32fefc04f4b8")
							}
						},
						Value = SourceConfiguration
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Destination Configuration",
							Guids = new List<Guid>()
							{
								new Guid("b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a")
							}
						},
						Value = DestinationConfiguration
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Source Provider",
							Guids = new List<Guid>()
							{
								new Guid("dc902551-2c9c-4f41-a917-41f4a3ef7409")
							}
						},
						Value = SourceProvider
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Schedule Rule",
							Guids = new List<Guid>()
							{
								new Guid("000f25ef-d714-4671-8075-d2a71cac396b")
							}
						},
						Value = ScheduleRule
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Overwrite Fields",
							Guids = new List<Guid>()
							{
								new Guid("0cae01d8-0dc3-4852-9359-fb954215c36f")
							}
						},
						Value = OverwriteFields
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Destination Provider",
							Guids = new List<Guid>()
							{
								new Guid("d6f4384a-0d2c-4eee-aab8-033cc77155ee")
							}
						},
						Value = DestinationProvider
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Job History",
							Guids = new List<Guid>()
							{
								new Guid("14b230cf-a505-4dd3-b05c-c54d05e62966")
							}
						},
						Value = JobHistory
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "LogErrors",
							Guids = new List<Guid>()
							{
								new Guid("0319869e-37aa-499c-a95b-6d8d0e96a711")
							}
						},
						Value = LogErrors
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "EmailNotificationRecipients",
							Guids = new List<Guid>()
							{
								new Guid("1bac59db-f7bf-48e0-91d4-18cf09ff0e39")
							}
						},
						Value = EmailNotificationRecipients
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Has Errors",
							Guids = new List<Guid>()
							{
								new Guid("a9853e55-0ba0-43d8-a766-747a61471981")
							}
						},
						Value = HasErrors
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Type",
							Guids = new List<Guid>()
							{
								new Guid("e646016e-5df6-4440-b218-18a00926d002")
							}
						},
						Value = Type
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Secured Configuration",
							Guids = new List<Guid>()
							{
								new Guid("48b0a4cb-bc21-45b5-b124-76ae27e03c42")
							}
						},
						Value = SecuredConfiguration
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Promote Eligible",
							Guids = new List<Guid>()
							{
								new Guid("bf85f332-8c8f-4c69-86fd-6ce4c567ebf9")
							}
						},
						Value = PromoteEligible
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Name",
							Guids = new List<Guid>()
							{
								new Guid("d534f433-dd92-4a53-b12d-bf85472e6d7a")
							}
						},
						Value = Name
					},
				}
			};
		}

		public IntegrationPoint ToRdo()
		{
			return new IntegrationPoint
			{
				RelativityObject = ToRelativityObject(),
				ArtifactId = ArtifactId,
				ParentArtifactId = ParenObjectArtifactId,
				NextScheduledRuntimeUTC = NextScheduledRuntimeUTC,
				LastRuntimeUTC = LastRuntimeUTC, //
				FieldMappings = FieldMappings,
				EnableScheduler = EnableScheduler,
				SourceConfiguration = SourceConfiguration,
				DestinationConfiguration = DestinationConfiguration,
				SourceProvider = SourceProvider,
				ScheduleRule = ScheduleRule,
				OverwriteFields = OverwriteFields,
				DestinationProvider = DestinationProvider,
				JobHistory = JobHistory,//
				LogErrors = LogErrors,
				EmailNotificationRecipients = EmailNotificationRecipients,
				HasErrors = HasErrors,
				Type = Type,
				SecuredConfiguration = SecuredConfiguration,//
				PromoteEligible = PromoteEligible,
				Name = Name
			};
		}
	}
}
