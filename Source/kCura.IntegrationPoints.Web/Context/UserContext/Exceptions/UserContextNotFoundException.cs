using System;

namespace kCura.IntegrationPoints.Web.Context.UserContext.Exceptions
{
    public class UserContextNotFoundException : InvalidOperationException
    {
        public UserContextNotFoundException(string propertyName) : base($"{propertyName} not found in user context data")
        {
        }
    }
}
