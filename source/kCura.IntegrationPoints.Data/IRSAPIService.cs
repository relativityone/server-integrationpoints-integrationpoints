using System;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data
{
	public partial interface IRSAPIService
	{
		IGenericLibrary<IntegrationPoint> IntegrationPointLibrary { get; set; }
		IGenericLibrary<SourceProvider> SourceProviderLibrary { get; set; }
	}
}
