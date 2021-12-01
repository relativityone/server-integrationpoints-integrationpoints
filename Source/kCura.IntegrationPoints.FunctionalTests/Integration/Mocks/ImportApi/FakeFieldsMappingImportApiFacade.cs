using System;
using System.Collections.Generic;
using kCura.Relativity.ImportAPI.Data;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.ImportApi
{
    class FakeFieldsMappingImportApiFacade : IFieldsMappingImportApiFacade
    {
        public IEnumerable<FieldTest> Fields { get; set; }

        public IEnumerable<Field> GetWorkspaceFields(int workspaceId, int documentArtifactTypeId)
        {
            throw new NotImplementedException();
        }
    }
}
