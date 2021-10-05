using System.IO;
using System.Threading.Tasks;
using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.System.Core;

// ReSharper disable once CheckNamespace
// No namespace applies this to the whole assembly
public class PerformanceTestsSetup : InstanceTestsSetup
{
	public override async Task RunBeforeAnyTests()
	{
		Logger.LogInformation("PerformanceTestsSetup: BeforeAnyTests");
		await base.RunBeforeAnyTests().ConfigureAwait(false);

		if (!AppSettings.IsSettingsFileSet)
		{
			throw new FileNotFoundException(
				@"*.runsettings File is not set. Set File in VS -> Test -> Select Settings File. If file doesn't exist generate it by .\Development Scripts\New-TestSettings.ps1.");
		}

		ARMHelper.CreateInstance();
	}
}
