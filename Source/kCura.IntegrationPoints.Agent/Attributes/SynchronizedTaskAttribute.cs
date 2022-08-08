using System;

namespace kCura.IntegrationPoints.Agent.Attributes
{
    /// <summary>
    /// If a task implements this tag, it will be synchronized, only allowing one active job in the queue at a time.
    /// </summary>
    public class SynchronizedTaskAttribute : Attribute
    {
    }
}
