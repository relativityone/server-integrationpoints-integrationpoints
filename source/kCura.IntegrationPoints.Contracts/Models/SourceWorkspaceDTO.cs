﻿using System;

namespace kCura.IntegrationPoints.Contracts.Models
{
	public class SourceWorkspaceDTO
	{
		private static readonly Guid _objectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		public static Guid ObjectTypeGuid { get { return _objectTypeGuid; } }
		public int ArtifactTypeId { get; set; } 
		public int ArtifactId { get; set; } 
		public string Name { get; set; }
		public int SourceCaseArtifactId { get; set; }
		public string SourceCaseName { get; set; }

		public static class Fields
		{
			private static readonly Guid _caseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
			private static readonly Guid _caseNameFieldNameGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
			private static readonly Guid _sourceWorkspaceFieldOnDocumentGuid = new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f");
			public static Guid CaseIdFieldNameGuid => _caseIdFieldNameGuid;
			public static Guid CaseNameFieldNameGuid => _caseNameFieldNameGuid;
			public static Guid SourceWorkspaceFieldOnDocumentGuid => _sourceWorkspaceFieldOnDocumentGuid;
		}
	}
}