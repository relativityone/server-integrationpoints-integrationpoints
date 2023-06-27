using System;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface INativeFilePathReader : IDisposable
    {
        bool Read();

        string GetCurrentNativeFilePath();
    }
}
