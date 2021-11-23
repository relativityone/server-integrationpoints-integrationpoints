using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    /// <summary>
    ///     Enable toggle to use Keplers instead of legacy WebAPI.
    /// </summary>
    [DefaultValue(false)]
    [Description("Enable toggle to use Keplers instead of legacy WebAPI.", "Adler Sieben")]
    public class EnableKeplerizedImportAPIToggle : IToggle
    {
    }
}
