using System;
using System.Reflection;
using Relativity.API;

namespace Relativity.Sync.Telemetry
{
	/// <inheritdoc/>
	internal class EnvironmentPropertyProvider : IEnvironmentPropertyProvider
	{
		private static IEnvironmentPropertyProvider _instance;
		private const string _GET_INSTANCE_NAME_SQL = "SELECT [Value] from [EDDS].[eddsdbo].[Configuration] WHERE [Section] = 'kCura.LicenseManager' AND [Name] = 'Instance'";

		/// <summary>
		///     Returns the static instance of <see cref="IEnvironmentPropertyProvider"/>.
		/// </summary>
		/// <param name="helper">API helper used to create the instance, if necessary</param>
		/// <returns>AppDomain-/process-wide instance of <see cref="IEnvironmentPropertyProvider"/></returns>
		public static IEnvironmentPropertyProvider GetInstance(IHelper helper)
		{
			if (_instance == null)
			{
				string relativityInstanceName = helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<string>(_GET_INSTANCE_NAME_SQL);
				_instance = new EnvironmentPropertyProvider(relativityInstanceName);
			}

			return _instance;
		}

		private EnvironmentPropertyProvider(string instanceName)
		{
			InstanceName = instanceName;
		}

		/// <inheritdoc/>
		public string InstanceName { get; }

		/// <inheritdoc/>
		public string CallingAssembly => Assembly.GetCallingAssembly().GetName().Name;
	}
}
