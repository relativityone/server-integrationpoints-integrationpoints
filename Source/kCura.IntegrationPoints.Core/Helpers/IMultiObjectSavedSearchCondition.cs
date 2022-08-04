using System;
using System.Collections.Generic;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface IMultiObjectSavedSearchCondition
    {
        CriteriaBase CreateConditionForMultiObject(Guid fieldGuid, CriteriaConditionEnum conditionEnum, List<int> values);
    }
}