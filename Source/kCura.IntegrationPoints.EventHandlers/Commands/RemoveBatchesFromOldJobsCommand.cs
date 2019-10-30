using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class RemoveBatchesFromOldJobsCommand : IEHCommand
	{
		private readonly IEHContext _context;

		public RemoveBatchesFromOldJobsCommand(IEHContext context)
		{
			_context = context;
		}

		public void Execute()
		{
			throw new System.NotImplementedException();
		}
	}
}