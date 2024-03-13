namespace Relativity.IntegrationPoints.Contracts
{
	/// <summary>
	/// Makes an initial call into an application domain to perform setup work.
	/// </summary>
    /// <remarks>Only a single class per library should implement this interface. This class must contain an empty constructor.</remarks>
	public interface IStartUp
	{
		/// <summary>
		/// Performs setup work required prior to running a provider.
		/// </summary>
		void Execute();
	}
}
