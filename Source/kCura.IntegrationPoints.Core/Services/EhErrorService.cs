
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services
{
	public class EhErrorService : ErrorServiceBase
	{
		public override string DefaultSourceName { get; } = "Event Handler";

		public EhErrorService(CreateErrorRdoQuery createError) : base(createError) { }
		
	}
}
