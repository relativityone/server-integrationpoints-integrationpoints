namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	// TODO: Remove this class. Push and pull are no longer different.
	/// <summary>
	/// This exists in the event that the pull behavior differs from the push behavior
	/// </summary>
	public class RdoSynchronizerPush : RdoSynchronizerBase
	{
		public RdoSynchronizerPush(IRelativityFieldQuery fieldQuery, IImportApiFactory factory)
			: base(fieldQuery, factory)
		{
		}
	}
}