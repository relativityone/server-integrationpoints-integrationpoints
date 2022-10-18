using System;
using Relativity.Import.V1.Models.Sources;

namespace Relativity.Sync.Executors
{
    internal class LoadFile : ILoadFile
    {
        public LoadFile(Guid id, string path, DataSourceSettings settings)
        {
            Id = id;
            Path = path;
            Settings = settings;
        }

        public Guid Id { get; private set; }

        public string Path { get; private set; }

        public DataSourceSettings Settings { get; private set; }
    }
}
