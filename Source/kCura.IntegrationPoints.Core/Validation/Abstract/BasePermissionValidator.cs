using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
    public abstract class BasePermissionValidator : BasePartsValidator<IntegrationPointProviderValidationModel>, IPermissionValidator
    {
        protected readonly ISerializer Serializer;
        protected readonly IServiceContextHelper ContextHelper;

        protected BasePermissionValidator(ISerializer serializer, IServiceContextHelper contextHelper)
        {
            Serializer = serializer;
            ContextHelper = contextHelper;
        }
    }
}
