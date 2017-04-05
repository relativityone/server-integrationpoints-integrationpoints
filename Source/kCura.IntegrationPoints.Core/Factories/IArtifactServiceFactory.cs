using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IArtifactServiceFactory
	{
		IArtifactService CreateArtifactService(IHelper helper, IHelper targetHelper);
	}
}
