using System.Linq;
using kCura.WinEDDS;
using kCura.WinEDDS.Core.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExtendedFieldNameProvider : FieldNameProvider
	{
		private readonly ExportSettings _settings;

		public ExtendedFieldNameProvider(ExportSettings settings)
		{
			_settings = settings;
		}

		public override string GetDisplayName(ViewFieldInfo fieldInfo)
		{
			if (_settings.SelViewFieldIds.Any())
			{
				if (_settings.SelViewFieldIds.ContainsKey(fieldInfo.AvfId))
				{
					// It should point to renamed column text if DisplayedName was changed by the user
					return _settings.SelViewFieldIds[fieldInfo.AvfId].DisplayName;
				}
			}
			return base.GetDisplayName(fieldInfo);
		}
	}
}
