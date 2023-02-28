using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoint.Tests.Core.Exceptions
{
    [Serializable]
    public class TestException : Exception
    {
        public TestException()
        {
        }

        public TestException(string message) : base(message)
        {
        }

        public TestException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
