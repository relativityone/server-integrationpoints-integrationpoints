namespace kCura.IntegrationPoints.Services.Helpers
{
	public interface IBackwardCompatibility
	{
		void FixIncompatibilities(IntegrationPointModel integrationPointModel, string overwriteFieldsName);
	}
}