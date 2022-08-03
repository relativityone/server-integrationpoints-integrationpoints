using System;
using System.Collections.Generic;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class MultiObjectSavedSearchCondition : IMultiObjectSavedSearchCondition
    {
        public CriteriaBase CreateConditionForMultiObject(Guid fieldGuid, CriteriaConditionEnum conditionEnum, List<int> values)
        {
            var fieldIdentifier = new FieldRef(new List<Guid> {fieldGuid});

            //Create main condition
            var criteria = new Criteria
            {
                Condition = new CriteriaCondition(fieldIdentifier, conditionEnum, values),
                BooleanOperator = BooleanOperatorEnum.And
            };

            //Aggregate condtion with CriteriaCollection
            var criteriaCollection = new CriteriaCollection();
            criteriaCollection.Conditions.Add(criteria);

            //MultiObjects require condition to be aggregated into additional CriteriaCondition
            var parentCriteria = new Criteria
            {
                Condition = new CriteriaCondition(fieldIdentifier, CriteriaConditionEnum.In, criteriaCollection)
            };
            return parentCriteria;
        }
    }
}