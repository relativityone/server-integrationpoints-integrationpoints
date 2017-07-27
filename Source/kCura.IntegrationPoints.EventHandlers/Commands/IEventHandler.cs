using System;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public interface IEventHandler
	{
		IEHContext Context { get; }

		string SuccessMessage { get; }

		string FailureMessage { get; }

		Type CommandType { get; }
	}
}