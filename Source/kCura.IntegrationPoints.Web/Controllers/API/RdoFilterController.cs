using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Extensions;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class RdoFilterController : ApiController
    {
        private readonly ICaseServiceContext _serviceContext;
        private readonly IObjectTypeRepository _objectTypeRepository;
        private readonly IRdoFilter _rdoFilter;
        private readonly IAPILog _logger;

        public RdoFilterController(ICaseServiceContext serviceContext, IRdoFilter rdoFilter, IObjectTypeRepository objectTypeRepository, IHelper helper)
        {
            _serviceContext = serviceContext;
            _rdoFilter = rdoFilter;
            _objectTypeRepository = objectTypeRepository;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoFilterController>();
        }

        // GET api/<controller>
        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve accessible RDO list.")]
        public HttpResponseMessage GetAllViewableRdos()
        {
            const int slidingExpirationHours = 1;
            string key = $"{_serviceContext.WorkspaceID}_{_serviceContext.WorkspaceUserID}";

            List<ViewableRdo> viewableRdos = HttpRuntime
                .Cache
                .GetOrInsert(key, GetViewableRdos, TimeSpan.FromHours(slidingExpirationHours));

            return Request.CreateResponse(HttpStatusCode.OK, viewableRdos);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to object type.")]
        public HttpResponseMessage Get(int id)
        {
            Domain.Models.ObjectTypeDTO list = _objectTypeRepository.GetObjectType(id);

            return Request.CreateResponse(HttpStatusCode.OK, new { name = list.Name, value = list.DescriptorArtifactTypeId });
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to get default rdo type id")]
        public HttpResponseMessage GetDefaultRdoTypeId()
        {
            return Request.CreateResponse(HttpStatusCode.OK, (int)ArtifactType.Document);
        }

        private List<ViewableRdo> GetViewableRdos()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                List<ViewableRdo> viewableRdos = _rdoFilter
                .GetAllViewableRdos()
                .GetAwaiter().GetResult()
                .Select(x => new ViewableRdo
                {
                    Name = x.Name,
                    Value = x.DescriptorArtifactTypeId,
                    BelongsToApplication = x.BelongsToApplication
                })
                .ToList();

                stopwatch.Stop();

                _logger.LogInformation("Retrieved {count} RDOs in {seconds} seconds.", viewableRdos.Count, Math.Round(stopwatch.Elapsed.TotalSeconds, 2));

                return viewableRdos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving viewable RDOs");
                throw;
            }
        }

        private class ViewableRdo
        {
            public string Name { get; set; }
            public int? Value { get; set; }
            public bool BelongsToApplication { get; set; }
        }
    }
}