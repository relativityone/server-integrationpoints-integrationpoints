using System;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts.Internals
{
	internal interface IProviderAggregatedInterfaces : IDataSourceProvider, IEmailBodyData, IDisposable
	{
	}
}
