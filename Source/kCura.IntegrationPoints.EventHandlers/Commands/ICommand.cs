namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public interface ICommand
	{
		void Execute();

		string SuccessMessage { get; }
		string FailureMessage { get; }
	}
}