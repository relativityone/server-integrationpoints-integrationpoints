using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class IntegrationPointProfileTest : RdoTestBase
    {
		public static Guid FieldsMappingGuid { get; } = new Guid("8ae37734-29d1-4441-b5d8-483134f98818");

		public DateTime? NextScheduledRuntimeUTC { get; set; }

		public string FieldMappings { get; set; } = "[]";

		public bool? EnableScheduler { get; set; }

		public string SourceConfiguration { get; set; } = "{}";

		public string DestinationConfiguration { get; set; } = "{}";

		public int? SourceProvider { get; set; }

		public string ScheduleRule { get; set; }

		public ChoiceRef OverwriteFields { get; set; } = OverwriteFieldsChoices.IntegrationPointAppendOnly;

		public int? DestinationProvider { get; set; }

		public bool? LogErrors { get; set; }

		public string EmailNotificationRecipients { get; set; }

		public int Type { get; set; }

		public bool? PromoteEligible { get; set; }

		public string Name { get; set; }

		public IntegrationPointProfileTest() : base("IntegrationPointProfile")
		{
			Name = $"Integration Point Profile (Artifact ID {ArtifactId})";
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
								new Guid("a3c48572-4ec7-4e06-b57f-1c1681cd07d1")
							}
						},
						Value = NextScheduledRuntimeUTC
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
								new Guid("bc2e19fd-c95c-4f1c-b4a9-1692590cef8e")
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
								new Guid("9ed96d44-7767-46f5-a67f-28b48b155ff2")
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
								new Guid("5d9e425a-b59c-4119-9ceb-73665a5e7049")
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
								new Guid("60d3de54-f0d5-4744-a23f-a17609edc537")
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
								new Guid("35b6c8b8-5b0c-4660-bfdd-226e424edeb5")
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
								new Guid("e0a14a2c-0bb6-47ad-a34f-26400258a761")
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
								new Guid("7d9e7944-bf13-4c4f-a9eb-5f60e683ec0c")
							}
						},
						Value = DestinationProvider
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "LogErrors",
							Guids = new List<Guid>()
							{
								new Guid("b582f002-00fe-4c44-b721-859dd011d4fd")
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
								new Guid("b72f5bb9-2e07-45f0-903a-b20d3a17958c")
							}
						},
						Value = EmailNotificationRecipients
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Type",
							Guids = new List<Guid>()
							{
								new Guid("8999dd19-c67c-43e3-88c0-edc989e224cc")
							}
						},
						Value = Type
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Promote Eligible",
							Guids = new List<Guid>()
							{
								new Guid("4997cdb4-d8b0-4eaa-8768-061d82aaaccf")
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
								new Guid("ad0552e0-4511-4507-a60d-23c0c6d05972")
							}
						},
						Value = Name
					},
				}
			};
		}

		public IntegrationPointProfile ToRdo()
		{
			return new IntegrationPointProfile
			{
				RelativityObject = ToRelativityObject(),
				ArtifactId = ArtifactId,
				ParentArtifactId = ParenObjectArtifactId,
				NextScheduledRuntimeUTC = NextScheduledRuntimeUTC,
				FieldMappings = FieldMappings,
				EnableScheduler = EnableScheduler,
				SourceConfiguration = SourceConfiguration,
				DestinationConfiguration = DestinationConfiguration,
				SourceProvider = SourceProvider,
				ScheduleRule = ScheduleRule,
				OverwriteFields = OverwriteFields,
				DestinationProvider = DestinationProvider,
				LogErrors = LogErrors,
				EmailNotificationRecipients = EmailNotificationRecipients,
				Type = Type,
				PromoteEligible = PromoteEligible,
				Name = Name
			};
		}
	}
}
