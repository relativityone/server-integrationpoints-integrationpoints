using kCura.IntegrationPoints.Domain.Exceptions;
using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.LDAPProvider
{
    [Serializable]
    public class LDAPProviderException : IntegrationPointsException
    {
        public LDAPProviderException() : this(string.Empty)
        {
        }

        public LDAPProviderException(string message) : this(message, null)
        {
        }

        public LDAPProviderException(string message, Exception innerException) : base(message, innerException)
        {
            Initialize();
        }

        protected LDAPProviderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Initialize();
        }

        private void Initialize()
        {
            ExceptionSource = IntegrationPointsExceptionSource.LDAP;
            ShouldAddToErrorsTab = true;
        }
    }
}
