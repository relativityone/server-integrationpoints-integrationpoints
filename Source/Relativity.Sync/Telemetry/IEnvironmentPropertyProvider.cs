namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Provides enviroment-wide property values for use in logging, telemetry, etc.
	/// </summary>
	internal interface IEnvironmentPropertyProvider
	{
		/// <summary>
		///     Name of the executing Relativity instance.
		/// </summary>
		string InstanceName { get; }

		/// <summary>
		///     Simple name of the calling assembly.
		/// </summary>
		string CallingAssembly { get; }
	}
}
