using System;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain.Wrappers
{
	internal interface IProviderAggregatedInterfaces : IDataSourceProvider, IEmailBodyData, IDisposable
	{
	}
}
