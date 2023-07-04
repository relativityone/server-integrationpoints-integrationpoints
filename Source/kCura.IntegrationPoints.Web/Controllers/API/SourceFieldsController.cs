﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Extensions;
using kCura.IntegrationPoints.Web.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class SourceOptions
    {
        public Guid Type { get; set; }
        public object Options { get; set; }
        public string Credentials { get; set; }
    }

    public class SourceFieldsController : ApiController
    {
        private readonly IGetSourceProviderRdoByIdentifier _sourceProviderIdentifier;
        private readonly IDataProviderFactory _factory;

        public SourceFieldsController(
            IGetSourceProviderRdoByIdentifier sourceProviderIdentifier,
            IDataProviderFactory factory)
        {
            _sourceProviderIdentifier = sourceProviderIdentifier;
            _factory = factory;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve source fields.")]
        public HttpResponseMessage Get(SourceOptions data)
        {
            Data.SourceProvider providerRdo = _sourceProviderIdentifier.Execute(data.Type);
            Guid applicationGuid = new Guid(providerRdo.ApplicationIdentifier);
            IDataSourceProvider provider = _factory.GetDataProvider(applicationGuid, data.Type);
            List<FieldEntry> fields = provider.GetFields(new DataSourceProviderConfiguration(data.Options.ToString(), data.Credentials)).OrderBy(x => x.DisplayName).ToList();

            List<ClassifiedFieldDTO> result = new List<ClassifiedFieldDTO>();
            int idx = 0;
            foreach (FieldEntry field in fields)
            {
                ClassifiedFieldDTO classifiedFieldDTO = new ClassifiedFieldDTO
                {
                    ClassificationLevel = ClassificationLevel.AutoMap,
                    FieldIdentifier = int.TryParse(field.FieldIdentifier, out _) ? field.FieldIdentifier : idx.ToString(),
                    Name = field.ActualName,
                    Type = field.Type,
                    IsIdentifier = field.IsIdentifier,
                    IsRequired = field.IsRequired
                };

                result.Add(classifiedFieldDTO);
                idx++;
            }

            return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }
    }
}
