using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.EventHandlers
{
	public abstract class PreLoadEventHandlerBase : kCura.EventHandler.PreLoadEventHandler
	{
		private IServiceContext _context;

		public IServiceContext ServiceContext
		{
			get { return _context ?? (_context = new ServiceContext
			{
				SqlContext = base.Helper.GetDBContext(base.Helper.GetActiveCaseID()),
				WorkspaceID = base.Application.ArtifactID
			}); }
		}
	}
}
