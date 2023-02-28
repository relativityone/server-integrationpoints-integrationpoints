using System;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions
{
    public class WorkspaceIdNotFoundException : InvalidOperationException
    {
        public WorkspaceIdNotFoundException() : base("WorkspaceId not found")
        {
        }
    }
}
