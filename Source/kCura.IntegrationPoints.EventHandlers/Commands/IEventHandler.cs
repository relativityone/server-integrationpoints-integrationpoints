using System;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public interface IEventHandlerEx : IEventHandler
    {
        string SuccessMessage { get; }
        string FailureMessage { get; }
    }

    public interface IEventHandler
    {
        IEHContext Context { get; }

        Type CommandType { get; }
    }
}