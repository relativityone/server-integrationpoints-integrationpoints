﻿using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using System.Web.Mvc;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Extensions;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class IntegrationPointsController : IntegrationPointBaseController
    {
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ICamelCaseSerializer _serializer;

        public IntegrationPointsController(
            IObjectTypeRepository objectTypeRepository,
            IRepositoryFactory repositoryFactory,
            ITabService tabService,
            IIntegrationPointService integrationPointService,
            IWorkspaceContext workspaceIdProvider,
            IUserContext userContext,
            ICamelCaseSerializer serializer
        ) : base(
            objectTypeRepository,
            repositoryFactory,
            tabService,
            workspaceIdProvider,
            userContext
        )
        {
            _integrationPointService = integrationPointService;
            _serializer = serializer;
        }

        protected override string ObjectTypeGuid => ObjectTypeGuids.IntegrationPoint;
        protected override string ObjectType => ObjectTypes.IntegrationPoint;
        protected override string APIControllerName => Core.Constants.IntegrationPoints.API_CONTROLLER_NAME;

        protected override IntegrationPointWebModelBase GetIntegrationPoint(int id)
        {
            return _integrationPointService.Read(id).ToWebModel(_serializer);
        }

        public ActionResult SaveAsProfileModal()
        {
            return PartialView();
        }
    }
}
