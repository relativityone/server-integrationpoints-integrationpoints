using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.SourceTypes;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[kCura.EventHandler.CustomAttributes.Description("Update Integration Points Entities - On Every Install")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("02ec5d64-208a-44fb-a5e3-c3a1103e7da7")]
	public class RunEveryTimeInstaller : kCura.EventHandler.PostInstallEventHandler
	{
		private IServiceContext _context;

		public IServiceContext ServiceContext
		{
			get { return _context ?? (_context = ServiceContextFactory.CreateServiceContext(base.Helper, this.ApplicationArtifactId)); }
			set { _context = value; }
		}

		public override Response Execute()
		{
			new LdapSourceTypeCreator(ServiceContext).CreateOrUpdateLdapSourceType();
			return new Response
			{
				Success = true
			};
		}

	}
}
