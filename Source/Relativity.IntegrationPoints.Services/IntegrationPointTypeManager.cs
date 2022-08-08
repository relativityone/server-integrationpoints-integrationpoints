using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services
{
    public class IntegrationPointTypeManager : KeplerServiceBase, IIntegrationPointTypeManager
    {
        private Installer _installer;

        /// <summary>
        ///     For testing purposes only
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="permissionRepositoryFactory"></param>
        /// <param name="container"></param>
        internal IntegrationPointTypeManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
            : base(logger, permissionRepositoryFactory, container)
        {
        }

        public IntegrationPointTypeManager(ILog logger) : base(logger)
        {
        }

        protected override Installer Installer => _installer ?? (_installer = new IntegrationPointTypeManagerInstaller());

        public void Dispose()
        {
        }

        public async Task<IList<IntegrationPointTypeModel>> GetIntegrationPointTypes(int workspaceArtifactId)
        {
            CheckPermissions(nameof(GetIntegrationPointTypes), workspaceArtifactId, new[]
            {
                new PermissionModel(ObjectTypeGuids.IntegrationPointTypeGuid, ObjectTypes.IntegrationPointType, ArtifactPermission.View)
            });
            try
            {
                using (var container = GetDependenciesContainer(workspaceArtifactId))
                {
                    var integrationPointTypeRepository = container.Resolve<IIntegrationPointTypeRepository>();
                    return await Task.Run(() => integrationPointTypeRepository.GetIntegrationPointTypes()).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                LogException(nameof(GetIntegrationPointTypes), e);
                throw CreateInternalServerErrorException();
            }
        }
    }
}