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
			get { return _context ?? (_context = ServiceContextFactory.CreateServiceContext(base.Helper)); }
			set { _context = value; }
		}
	}
}
