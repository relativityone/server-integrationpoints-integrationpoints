using System;
using System.Data;
using System.Reflection;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class Agent
	{
		public int ArtifactId { get; set; }
		public int AgentTypeId { get; set; }
		public string Name { get; set; }
		public string FullNamespace { get; set; }
		public Guid AgentGuid { get; set; }

		public DataRow AsRow()
		{
			var row = DatabaseSchema.AgentSchema()
				.NewRow();

			row["AgentTypeID"] = AgentTypeId;
			row["Name"] = Name;
			row["Fullnamespace"] = FullNamespace;
			row["Guid"] = AgentGuid;

			return row;
		}
	}
}
