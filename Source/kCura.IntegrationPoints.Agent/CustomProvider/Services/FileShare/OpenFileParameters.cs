using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare
{
    public class OpenFileParameters
    {
        public string Path { get; set; }

        public OpenBehavior OpenBehavior { get; set; }

        public ReadWriteMode ReadWriteMode { get; set; }

        public OpenFileOptions OpenFileOptions { get; set; }
    }
}
