using System;

namespace kCura.IntegrationPoints.Agent.Exceptions
{
    [Serializable]
    public class AgentDropJobException : Exception
    {
        public AgentDropJobException()
        {
        }

        public AgentDropJobException(string message) : base(message)
        {
        }

        public AgentDropJobException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AgentDropJobException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
