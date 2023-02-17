using kCura.IntegrationPoints.Data.Queries;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
    public class CustomPageErrorService : ErrorServiceBase
    {
        public override string TargetName { get; } = "Custom Page";

        public CustomPageErrorService(CreateErrorRdoQuery createError, IAPILog log) : base(createError, log) { }
    }
}
