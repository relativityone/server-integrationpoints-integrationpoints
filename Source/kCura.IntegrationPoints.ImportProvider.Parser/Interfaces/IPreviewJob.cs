using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.WinEDDS;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IPreviewJob
    {
        void Init(LoadFile loadFile, ImportPreviewSettings settings);
        void StartRead();
        void DisposePreviewJob();

        ImportPreviewTable PreviewTable { get;}

        bool IsComplete { get; }
        bool IsFailed { get; }
        string ErrorMessage { get; }
        long TotalBytes { get; }
        long BytesRead { get; }
        long StepSize { get; }
    }
}
