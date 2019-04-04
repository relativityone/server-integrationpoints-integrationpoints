﻿using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;

namespace kCura.IntegrationPoints.Core.Provider
{
	public interface IProviderInstaller
	{
		Task<Either<string, Unit>> InstallProvidersAsync(IEnumerable<IntegrationPoints.Contracts.SourceProvider> providersToInstall);
	}
}
