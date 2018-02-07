using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.FieldMapping;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IFieldCatalogService
    {
        ExternalMapping[] GetAllFieldCatalogMappings(int workspaceId);
    }
}
