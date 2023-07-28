using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Services.Helpers;

namespace Relativity.IntegrationPoints.Services.Repositories.Implementations
{
    public abstract class IntegrationPointAccessorBase
    {
        private readonly IBackwardCompatibility _backwardCompatibility;

        protected ICaseServiceContext Context { get; }

        protected IntegrationPointAccessorBase(IBackwardCompatibility backwardCompatibility, ICaseServiceContext context)
        {
            _backwardCompatibility = backwardCompatibility;
            Context = context;
        }

        public abstract IList<OverwriteFieldsModel> GetOverwriteFieldChoices();

        protected int SaveIntegrationPoint(CreateIntegrationPointRequest request)
        {
            var overwriteFieldsName = GetOverwriteFieldsName(request.IntegrationPoint.OverwriteFieldsChoiceId);
            _backwardCompatibility.FixIncompatibilities(request.IntegrationPoint, overwriteFieldsName);
            return Save(request.IntegrationPoint, overwriteFieldsName);
        }

        protected abstract int Save(IntegrationPointModel model, string overwriteFieldsName);

        protected string GetOverwriteFieldsName(int overwriteFieldsId)
        {
            // TODO remove this hack when IntegrationPointModel will start using ChoiceId instead of ChoiceName
            return GetOverwriteFieldChoices().First(x => x.ArtifactId == overwriteFieldsId).Name;
        }

        protected void UpdateOverwriteFieldsChoiceId(IntegrationPointModel model)
        {
            IList<OverwriteFieldsModel> fieldsChoices = GetOverwriteFieldChoices();
            model.OverwriteFieldsChoiceId = fieldsChoices
                .First(x =>
                    x.Name == ((DestinationConfiguration)model.DestinationConfiguration).ImportOverwriteMode.ToString())
                .ArtifactId;
        }
    }
}
