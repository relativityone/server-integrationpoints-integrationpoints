using System.Security.Cryptography;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;

namespace kCura.IntegrationPoints.EventHandlers
{
	public abstract class PreLoadEventHandlerBase : kCura.EventHandler.PreLoadEventHandler
	{
		private ICaseServiceContext _context;

		public ICaseServiceContext ServiceContext
		{
			get
			{
				return _context ?? (_context = ServiceContextFactory.CreateCaseServiceContext(base.Helper, this.Application.ArtifactID));
			}
			set { _context = value; }
		}
	}
}
