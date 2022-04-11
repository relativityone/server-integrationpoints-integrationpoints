using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
	[DefaultValue(false)]
	[Description("Enable to allow NonAdmin users send document with LinksOnly options", "Adler Sieben")]
	public class EnableNonAdminSyncLinksToggle : IToggle
	{
	}
}