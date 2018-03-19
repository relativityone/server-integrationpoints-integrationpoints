namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public interface ISplitJsonObjectService
	{
		SplittedJsonObject Split(string jsonString, params string[] propertiesToExtract);
	}
}