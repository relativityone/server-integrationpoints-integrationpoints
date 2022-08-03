using System.Linq;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public class ExtendedFieldNameProvider : global::Relativity.DataExchange.Export.FieldNameProvider
    {
        private readonly ExportSettings _settings;

        public ExtendedFieldNameProvider(ExportSettings settings)
        {
            _settings = settings;
        }

        public override string GetDisplayName(ViewFieldInfo fieldInfo)
        {
            if (_settings.SelViewFieldIds != null && _settings.SelViewFieldIds.Any())
            {
                if (IsRenamedField(fieldInfo))
                {
                    // It should point to renamed column text if DisplayedName was changed by the user
                    return _settings.SelViewFieldIds[fieldInfo.AvfId].DisplayName;
                }
            }
            return base.GetDisplayName(fieldInfo);
        }

        private bool IsRenamedField(ViewFieldInfo fieldInfo)
        {
            return _settings.SelViewFieldIds.ContainsKey(fieldInfo.AvfId) && !(fieldInfo is CoalescedTextViewField);
        }
    }
}
