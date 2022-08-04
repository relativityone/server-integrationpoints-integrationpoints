using System;
using System.Data;

namespace kCura.ScheduleQueue.Core
{
    public class AgentTypeInformation
    {
        public Int32 AgentTypeID { get; private set; }
        public string Name { get; private set; }
        public string FullNamespace { get; private set; }
        public Guid GUID { get; private set; }

        public AgentTypeInformation(DataRow row)
        {
            AgentTypeID = row.Field<int>("AgentTypeID");
            Name = row.Field<string>("Name");
            FullNamespace = row.Field<string>("Fullnamespace");
            GUID = row.Field<Guid>("Guid");
        }
    }
}
