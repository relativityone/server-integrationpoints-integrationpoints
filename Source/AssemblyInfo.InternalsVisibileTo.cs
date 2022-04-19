using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Relativity.Sync.Tests.Common")]
[assembly:InternalsVisibleTo("Relativity.Sync.Tests.Unit")]
[assembly:InternalsVisibleTo("Relativity.Sync.Tests.Integration")]
[assembly:InternalsVisibleTo("Relativity.Sync.Tests.Performance")]
[assembly:InternalsVisibleTo("Relativity.Sync.Tests.System")]
[assembly:InternalsVisibleTo("Relativity.Sync.Tests.System.Core")]
[assembly:InternalsVisibleTo("kCura.IntegrationPoints.RelativitySync")]
[assembly:InternalsVisibleTo("kCura.IntegrationPoints.RelativitySync.Tests")]
[assembly:InternalsVisibleTo("kCura.IntegrationPoints.RelativitySync.Unit")]
[assembly:InternalsVisibleTo("kCura.IntegrationPoints.RelativitySync.Tests.Integration")]

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]


// this is needed for system tests to access nodes properties
[assembly: InternalsVisibleTo("Banzai.Autofac")]
[assembly: InternalsVisibleTo("Banzai")]
