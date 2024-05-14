namespace Relativity.Sync.Configuration
{
    /// <summary>
    /// Determines tagging option in job
    /// </summary>
    public enum TaggingOption
    {
        /// <summary>
        /// Documents will be tagged both in source and destination workspace.
        /// </summary>
        Enabled,

        /// <summary>
        /// Documents will be tagged only in destination workspace.
        /// </summary>
        DestinationOnly,

        /// <summary>
        /// Documents will not be tagged.
        /// </summary>
        Disabled
    }
}
