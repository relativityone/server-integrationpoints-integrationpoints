
using kCura.IntegrationPoints.Data.Queries;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
    public class EhErrorService : ErrorServiceBase
    {
        public override string TargetName { get; } = "Event Handler";

        public EhErrorService(CreateErrorRdoQuery createError, IAPILog log) : base(createError, log) { }
    }
}
