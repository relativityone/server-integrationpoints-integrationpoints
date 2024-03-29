﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Relativity.Sync.Progress
{
    [ExcludeFromCodeCoverage]
    internal sealed class EmptyProgress<T> : IProgress<T>
    {
        public void Report(T value)
        {
            // Method intentionally left empty.
        }
    }
}
