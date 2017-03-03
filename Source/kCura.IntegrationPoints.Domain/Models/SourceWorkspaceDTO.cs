using System;

namespace kCura.IntegrationPoints.Domain.Models
{
	public class SourceWorkspaceDTO
	{
		public static readonly Guid ObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		public int ArtifactTypeId { get; set; } 
		public int ArtifactId { get; set; } 
		public string Name { get; set; }
		public int SourceCaseArtifactId { get; set; }
		public string SourceCaseName { get; set; }
		public string SourceInstanceName { get; set; }

		public static class Fields
		{
			public static readonly Guid CaseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
			public static readonly Guid CaseNameFieldNameGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
			public static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
			public static readonly Guid SourceWorkspaceFieldOnDocumentGuid = new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f");
		}
	}
}