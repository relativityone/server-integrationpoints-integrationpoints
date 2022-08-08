using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoint.Tests.Core.Exceptions
{
    [Serializable]
    public class TestSetupException : TestException
    {
        public TestSetupException()
        {
        }

        public TestSetupException(string message) : base(message)
        {
        }

        public TestSetupException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TestSetupException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
