using System.Runtime.CompilerServices;
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
