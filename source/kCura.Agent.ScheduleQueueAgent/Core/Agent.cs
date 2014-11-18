using System;
using System.Data;

namespace kCura.Agent.ScheduleQueueAgent
{
	public class AgentInformation
	{
		public Int32 AgentID { get; private set; }
		public Int32 AgentTypeID { get; private set; }
		public string Name { get; private set; }
		public string FullNamespace { get; private set; }
		public Guid GUID { get; private set; }

		public AgentInformation(DataRow row)
		{
			AgentID = row.Field<int>("AgentID");
			AgentTypeID = row.Field<int>("AgentTypeID");
			Name = row.Field<string>("Name");
			FullNamespace = row.Field<string>("Fullnamespace");
			GUID = row.Field<Guid>("Guid");
		}
	}
}
