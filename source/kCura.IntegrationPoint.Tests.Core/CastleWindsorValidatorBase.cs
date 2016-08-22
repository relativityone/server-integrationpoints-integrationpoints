using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.MicroKernel;
using Castle.MicroKernel.Handlers;
using Castle.Windsor;
using Castle.Windsor.Diagnostics;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class CastleWindsorValidatorBase
	{
		public string AssemblyDirectory
		{
			get
			{
				var codeBase = Assembly.GetExecutingAssembly().CodeBase;

				var uri = new UriBuilder(codeBase);

				var path = Uri.UnescapeDataString(uri.Path);

				return Path.GetDirectoryName(path);
			}
		}

		public void CheckForPotentiallyMisconfiguredComponents(IWindsorContainer container)
		{
			IDiagnosticsHost host = (IDiagnosticsHost)container.Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
			IPotentiallyMisconfiguredComponentsDiagnostic diagnostics = host.GetDiagnostic<IPotentiallyMisconfiguredComponentsDiagnostic>();

			IHandler[] misconfiguredHandlers = diagnostics.Inspect();

			if (misconfiguredHandlers.Any())
			{
				var message = new StringBuilder();
				var inspector = new DependencyInspector(message);

				foreach (IHandler handler in misconfiguredHandlers)
				{
					IExposeDependencyInfo exposeDependency = handler as IExposeDependencyInfo;
					exposeDependency?.ObtainDependencyDetails(inspector);
				}

				if (!String.IsNullOrEmpty(message.ToString()))
				{
					throw new Exception(message.ToString());
				}
			}
		}
	}
}