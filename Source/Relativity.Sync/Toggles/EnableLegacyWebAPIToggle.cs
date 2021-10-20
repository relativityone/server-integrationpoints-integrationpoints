using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    [DefaultValue(false)]
    [Description("Enable toggle to use legacy WebAPI", "Adler Sieben")]
    public class EnableLegacyWebAPIToggle : IToggle
    {
    }
}
