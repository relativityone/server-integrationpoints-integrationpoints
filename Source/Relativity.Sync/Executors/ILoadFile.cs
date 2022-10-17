using System;
using Relativity.Import.V1.Models.Sources;

namespace Relativity.Sync.Executors
{
    internal interface ILoadFile
    {
        Guid Id { get; }

        string Path { get; }

        DataSourceSettings Settings { get; }
    }
}
