﻿using System;

namespace kCura.IntegrationPoints.Common.Helpers
{
    /// <summary>Provides a set of methods and properties that can be used to accurately measure elapsed time.</summary>
    public interface IStopwatch
    {
        /// <summary>Starts, or resumes, measuring elapsed time for an interval.</summary>
        void Start();

        /// <summary>Stops measuring elapsed time for an interval.</summary>
        void Stop();

        /// <summary>Gets the total elapsed time measured by the current instance.</summary>
        /// <returns>A read-only <see cref="System.TimeSpan" /> representing the total elapsed time measured by the current instance.</returns>
        TimeSpan Elapsed { get; }
    }
}