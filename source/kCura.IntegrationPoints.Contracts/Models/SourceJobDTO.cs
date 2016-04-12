using System;

namespace kCura.IntegrationPoints.Contracts.Models
{
	public class SourceJobDTO
	{
		private static readonly Guid _objectTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");
		public static Guid ObjectTypeGuid => _objectTypeGuid;
		public int SourceWorkspaceArtifactId { get; set; } 
		public int ArtifactId { get; set; }
		public string Name { get; set; }
		public int JobHistoryArtifactId { get; set; }
		public string JobHistoryName { get; set; }

		public static class Fields
		{
			private static readonly Guid _jobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
			private static readonly Guid _jobHistoryNameFieldGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
			private static readonly Guid _jobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");

			public static Guid JobHistoryIdFieldGuid => _jobHistoryIdFieldGuid;
			public static Guid JobHistoryNameFieldGuid => _jobHistoryNameFieldGuid;
			public static Guid JobHistoryFieldOnDocumentGuid => _jobHistoryFieldOnDocumentGuid;
		}
	}
}