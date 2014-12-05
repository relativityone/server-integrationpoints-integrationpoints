using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.Relativity.Client;
namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService : IRSAPIService
	{
		//public IIdentifierLibrary IDLibrary { get; set; }

		public IGenericLibrary<IntegrationPoint> IntegrationPointLibrary { get; set; }
		public IGenericLibrary<SourceProvider> SourceProviderLibrary { get; set; }

		public RSAPIService(IRSAPIClient client)
		{
			
		}

	}
}
