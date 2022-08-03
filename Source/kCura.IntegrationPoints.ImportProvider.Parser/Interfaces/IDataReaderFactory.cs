using System.Data;
using kCura.IntegrationPoints.Domain.Managers;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IDataReaderFactory
    {
        IDataReader GetDataReader(FieldMap[] fieldMaps, string options, IJobStopManager jobStopManager);
    }
}
