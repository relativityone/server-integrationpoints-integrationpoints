namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	//[Obsolete("Use IEHCommand instead.", false)]
	public interface ICommand
	{
		void Execute();

		string SuccessMessage { get; }
		string FailureMessage { get; }
	}
}