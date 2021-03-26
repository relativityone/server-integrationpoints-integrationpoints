using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class WorkspaceTest
	{
		public int ArtifactId { get; set; }
		public string Name { get; set; }

		public WorkspaceTest()
		{
			ArtifactId = ArtifactProvider.NextId();
		}

		public RelativityObject ToRelativityObject()
		{
			return new RelativityObject
			{
				ArtifactID = ArtifactId,
				FieldValues = new List<FieldValuePair>
				{
					new FieldValuePair
					{
						Field = new Field
						{
							Name = WorkspaceFieldsConstants.NAME_FIELD,
						},
						Value = Name
					}
				},
			};
		}
	}
}
