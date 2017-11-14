using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services
{
	public class CustomPageErrorService : ErrorServiceBase
	{
		public override string DefaultSourceName { get; } = "Custom Page";

		public CustomPageErrorService(CreateErrorRdoQuery createError) : base(createError) { }
	}
}