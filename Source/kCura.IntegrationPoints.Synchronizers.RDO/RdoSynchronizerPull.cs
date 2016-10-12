using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	/// <summary>
	/// This exists in the event that the pull behavior differs from the push behavior
	/// </summary>
	public class RdoSynchronizerPull : RdoSynchronizerBase
	{
		public RdoSynchronizerPull(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IHelper helper)
			: base(fieldQuery, factory, helper)
		{
		}
	}
}