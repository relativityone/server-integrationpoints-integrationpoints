using Relativity.Storage;

namespace kCura.IntegrationPoints.Core.Storage
{
    public class OpenFileParameters
    {
        public string Path { get; set; }

        public OpenBehavior OpenBehavior { get; set; }

        public ReadWriteMode ReadWriteMode { get; set; }

        public OpenFileOptions OpenFileOptions { get; set; }
    }
}
