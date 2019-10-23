using System;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace Relativity.IntegrationPoints.Contracts.Internals
{
	internal interface IProviderAggregatedInterfaces : IDataSourceProvider, IEmailBodyData, IDisposable
	{
	}
}
