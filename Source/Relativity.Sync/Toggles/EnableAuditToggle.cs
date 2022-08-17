using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    /// <summary>
    ///     Enables audits in IAPI jobs.
    /// </summary>
    [DefaultValue(true)]
    [Description("Enables audits in IAPI jobs.", "Adler Sieben")]
    public class EnableAuditToggle : IToggle
    {
    }
}
