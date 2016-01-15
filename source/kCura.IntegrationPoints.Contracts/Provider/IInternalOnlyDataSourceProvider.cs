using System.Runtime.CompilerServices;
using kCura.Relativity.Client;
using Relativity.API;

[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Web")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Core")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.DocumentTransferProvider")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints")] // ILMerge dll
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Contracts")] // ILMerge dll
[assembly: InternalsVisibleTo("kCura.DocumentTransferProvider")] // ILMerge dll
namespace kCura.IntegrationPoints.Contracts.Provider
{
	internal interface IInternalOnlyDataSourceProvider
	{
		IHelper Client { set; }
	}
}
