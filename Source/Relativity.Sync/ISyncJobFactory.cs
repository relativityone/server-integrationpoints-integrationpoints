using System.Collections.Generic;
using Autofac;

namespace Relativity.Sync
{
	/// <summary>
	///     Factory used to create <see cref="ISyncJob" />
	/// </summary>
	public interface ISyncJobFactory
	{
		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters);

		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		/// <param name="logger">Logger</param>
		ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, ISyncLog logger);

		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		/// <param name="configuration">Sync configuration</param>
		ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncConfiguration configuration);

		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="installers">Dependent <see cref="IInstaller"/>s that register dependencies in the <see cref="IContainer"/> for this job</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		ISyncJob Create(IContainer container, IEnumerable<IInstaller> installers, SyncJobParameters syncJobParameters);

		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="installers">Dependent <see cref="IInstaller"/>s that register dependencies in the <see cref="IContainer"/> for this job</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		/// <param name="configuration">Sync configuration</param>
		/// <param name="logger">Logger</param>
		ISyncJob Create(IContainer container, IEnumerable<IInstaller> installers, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger);
	}
}