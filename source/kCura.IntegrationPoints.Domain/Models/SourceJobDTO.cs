using System;

namespace kCura.IntegrationPoints.Domain.Models
{
	public class SourceJobDTO
	{
		public static readonly Guid ObjectTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");
		public int SourceWorkspaceArtifactId { get; set; } 
		public int ArtifactId { get; set; }
		public string Name { get; set; }
		public int JobHistoryArtifactId { get; set; }
		public string JobHistoryName { get; set; }

		public static class Fields
		{
			public static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
			public static readonly Guid JobHistoryNameFieldGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
			public static readonly Guid JobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");
		}
	}
}