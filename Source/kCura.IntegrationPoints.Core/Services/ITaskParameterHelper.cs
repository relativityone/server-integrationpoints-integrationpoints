using System;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface ITaskParameterHelper
    {
        Guid GetBatchInstance(Job job);
    }
}
