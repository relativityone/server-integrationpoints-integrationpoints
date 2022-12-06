using System;

namespace kCura.IntegrationPoints.Core.Exceptions
{
    public class KubernetesException : Exception
    {
        public KubernetesException(string message)
            : base(message)
        {
        }
    }
}
