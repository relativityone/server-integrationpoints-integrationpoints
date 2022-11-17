using System;
using Relativity.Import.V1.Models.Settings;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer.ImportAPI
{
    internal static class EnumExtensions
    {
        public static MultiFieldOverlayBehaviour ToMultiFieldOverlayBehaviour(this FieldOverlayBehavior behavior)
        {
            switch (behavior)
            {
                case FieldOverlayBehavior.MergeValues: return MultiFieldOverlayBehaviour.MergeAll;
                case FieldOverlayBehavior.ReplaceValues: return MultiFieldOverlayBehaviour.ReplaceAll;
                case FieldOverlayBehavior.UseFieldSettings: return MultiFieldOverlayBehaviour.UseRelativityDefaults;
                default: throw new NotSupportedException($"Unknown {nameof(FieldOverlayBehavior)} enum value: {behavior}");
            }
        }
    }
}
