using System;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public interface IEventHandler
	{
		IEHHelper Helper { get; }

		string SuccessMessage { get; }

		string FailureMessage { get; }

		Type CommandType { get; }
	}
}