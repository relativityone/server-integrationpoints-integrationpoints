using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.LDAPProvider
{
    [Serializable]
    public class LDAPProviderException : Exception
    {
        public LDAPProviderException()
        {
        }

        public LDAPProviderException(string message) : base(message)
        {
        }

        public LDAPProviderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LDAPProviderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
