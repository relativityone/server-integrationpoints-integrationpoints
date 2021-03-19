using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class JobHistoryTest
	{
		public const string BatchInstanceFieldName = "Batch Instance";

		public int ArtifactId { get; set; }

		public int WorkspaceId { get; set; }

		public int[] Documents { get; set; }

		public int[] IntegrationPoint { get; set; }

		public ChoiceRef JobStatus { get; set; }
		
		public int? ItemsTransferred { get; set; }

		public int? ItemsWithErrors { get; set; }

		public DateTime? StartTimeUTC { get; set; }

		public DateTime? EndTimeUTC { get; set; }

		public string BatchInstance { get; set; }

		public string DestinationWorkspace { get; set; }

		public long? TotalItems { get; set; }

		public int[] DestinationWorkspaceInformation { get; set; }

		public ChoiceRef JobType { get; set; }

		public string DestinationInstance { get; set; }

		public string FilesSize { get; set; }

		public string Overwrite { get; set; }

		public string JobID { get; set; }

		public string Name { get; set; }

		public JobHistoryTest()
		{
			ArtifactId = Artifact.NextId();
			Name = $"Job History (Artifact ID {ArtifactId})";
		}

		public RelativityObject ToRelativityObject()
		{
			return new RelativityObject()
			{
				ArtifactID = ArtifactId,
				Guids = new List<Guid>()
				{
					new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9")
				},
				Name = Name,
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Documents",
							Guids = new List<Guid>()
							{
								new Guid("5d99f717-3b5e-4773-9f51-9ca5d4c1a0fc")
							}
						},
						Value = Documents
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Integration Point",
							Guids = new List<Guid>()
							{
								new Guid("d3e791d3-2e21-45f4-b403-e7196bd25eea")
							}
						},
						Value = IntegrationPoint
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Job Status",
							Guids = new List<Guid>()
							{
								new Guid("5c28ce93-c62f-4d25-98c9-9a330a6feb52")
							}
						},
						Value = JobStatus
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Items Transferred",
							Guids = new List<Guid>()
							{
								new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c")
							}
						},
						Value = ItemsTransferred
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Items with Errors",
							Guids = new List<Guid>()
							{
								new Guid("c224104f-c1ca-4caa-9189-657e01d5504e")
							}
						},
						Value = ItemsWithErrors
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Start Time (UTC)",
							Guids = new List<Guid>()
							{
								new Guid("25b7c8ef-66d9-41d1-a8de-29a93e47fb11")
							}
						},
						Value = StartTimeUTC
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "End Time (UTC)",
							Guids = new List<Guid>()
							{
								new Guid("4736cf49-ad0f-4f02-aaaa-898e07400f22")
							}
						},
						Value = EndTimeUTC
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = BatchInstanceFieldName,
							Guids = new List<Guid>()
							{
								new Guid("08ba2c77-a9cd-4faf-a77a-be35e1ef1517")
							}
						},
						Value = BatchInstance
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Destination Workspace",
							Guids = new List<Guid>()
							{
								new Guid("ff01a766-b494-4f2c-9cbb-10a5ab163b8d")
							}
						},
						Value = DestinationWorkspace
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Total Items",
							Guids = new List<Guid>()
							{
								new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b")
							}
						},
						Value = TotalItems
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Destination Workspace Information",
							Guids = new List<Guid>()
							{
								new Guid("20a24c4e-55e8-4fc2-abbe-f75c07fad91b")
							}
						},
						Value = DestinationWorkspaceInformation
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Job Type",
							Guids = new List<Guid>()
							{
								new Guid("e809db5e-5e99-4a75-98a1-26129313a3f5")
							}
						},
						Value = JobType
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Destination Instance",
							Guids = new List<Guid>()
							{
								new Guid("6d91ea1e-7b34-46a9-854e-2b018d4e35ef")
							}
						},
						Value = DestinationInstance
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "FilesSize",
							Guids = new List<Guid>()
							{
								new Guid("d81817dc-91cb-44c4-b9b7-7c445da64f5a")
							}
						},
						Value = FilesSize
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Overwrite",
							Guids = new List<Guid>()
							{
								new Guid("42d49f5e-b0e7-4632-8d30-1c6ee1d97fa7")
							}
						},
						Value = Overwrite
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Job ID",
							Guids = new List<Guid>()
							{
								new Guid("77d797ef-96c9-4b47-9ef8-33f498b5af0d")
							}
						},
						Value = JobID
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Name",
							Guids = new List<Guid>()
							{
								new Guid("07061466-5fab-4581-979c-c801e8207370")
							}
						},
						Value = Name
					},
				}
			};
		}
	}
}