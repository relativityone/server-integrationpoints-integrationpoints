using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using Relativity.IntegrationPoints.Services.Helpers;

namespace Relativity.IntegrationPoints.Services.Repositories.Implementations
{
    public abstract class IntegrationPointBaseRepository
    {
        private readonly IBackwardCompatibility _backwardCompatibility;

        protected ICaseServiceContext Context { get; }

        protected IntegrationPointBaseRepository(IBackwardCompatibility backwardCompatibility, ICaseServiceContext context)
        {
            _backwardCompatibility = backwardCompatibility;
            Context = context;
        }

        protected int SaveIntegrationPoint(CreateIntegrationPointRequest request)
        {
            var overwriteFieldsName = GetOverwriteFieldsName(request.IntegrationPoint.OverwriteFieldsChoiceId);
            _backwardCompatibility.FixIncompatibilities(request.IntegrationPoint, overwriteFieldsName);
            return Save(request.IntegrationPoint, overwriteFieldsName);
        }

        public abstract int Save(IntegrationPointModel model, string overwriteFieldsName);
        
        protected string GetOverwriteFieldsName(int overwriteFieldsId)
        {
            //TODO remove this hack when IntegrationPointModel will start using ChoiceId instead of ChoiceName
            return GetOverwriteFieldChoices().First(x => x.ArtifactId == overwriteFieldsId).Name;
        }

        public abstract IList<OverwriteFieldsModel> GetOverwriteFieldChoices();
    }
}