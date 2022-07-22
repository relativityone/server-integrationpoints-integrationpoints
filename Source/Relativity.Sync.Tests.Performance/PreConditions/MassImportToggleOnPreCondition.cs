using System;
using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.Performance.PreConditions
{
    internal class MassImportToggleOnPreCondition : IPreCondition
    {
        private string _MASS_IMPORT_IMPROVEMENTS_TOGGLE = "Relativity.Core.Toggle.MassImportImprovementsToggle";

        public string Name => nameof(MassImportToggleOnPreCondition);
        
        public bool Check()
        {
            var toggleValue = ToggleHelper.GetToggleAsync(_MASS_IMPORT_IMPROVEMENTS_TOGGLE).GetAwaiter().GetResult();

            if (toggleValue == null)
                return false;

            return toggleValue.Value;
        }

        public FixResult TryFix()
        {
            ToggleHelper.SetToggleAsync(_MASS_IMPORT_IMPROVEMENTS_TOGGLE, true).GetAwaiter().GetResult();

            return Check()
                ? FixResult.Fixed()
                : FixResult.Error(new Exception($"{Name} - {_MASS_IMPORT_IMPROVEMENTS_TOGGLE} is still invalid after the fix"));
        }
    }
}
