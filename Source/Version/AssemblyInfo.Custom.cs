using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // NSubsitute requires this to mock internal interfaces
[assembly: InternalsVisibleTo("kCura.IntegrationPoints")]  //ILMerged dll
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Agent")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Agent.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Common.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Data")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Data.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Domain")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Domain.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Domain.Tests.Integration")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Core")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Core.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Core.Tests.Integration")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Data.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Data.Tests.Integration")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.DocumentTransferProvider")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.DocumentTransferProvider.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.EventHandlers")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.EventHandlers.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.EventHandlers.Tests.Integration")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.FtpProvider.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Management.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.SourceProviderInstaller")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Synchronizers.RDO")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Synchronizers.RDO.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoint.Tests.Core")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Web")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Web.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Web.Tests.Integration")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.Email.Tests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.PerformanceTests")]
[assembly: InternalsVisibleTo("kCura.IntegrationPoints.RelativitySync.Tests")]
[assembly: InternalsVisibleTo("Relativity.IntegrationPoints.FunctionalTests")]
[assembly: InternalsVisibleTo("Relativity.IntegrationPoints.Services.Tests")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
//[assembly: Guid("9808fab4-ae17-49c5-a74a-e34543378422")]
