using System;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
    public class SingleChoiceReferenceDto : IFieldValueDto
    {
        public Guid ChoiceValueGuid { get; }

        public object Value => new ChoiceRef
        {
            Guid = ChoiceValueGuid
        };
        
        public SingleChoiceReferenceDto(Guid choiceValueGuid)
        {
            ChoiceValueGuid = choiceValueGuid;
        }
    }
}
