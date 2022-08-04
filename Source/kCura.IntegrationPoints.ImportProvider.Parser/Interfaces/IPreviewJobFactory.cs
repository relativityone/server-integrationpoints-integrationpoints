using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IPreviewJobFactory
    {
        IPreviewJob GetPreviewJob(ImportPreviewSettings settings);
    }
}
