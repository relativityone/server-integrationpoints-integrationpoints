using System;

namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
    public class FieldUpdateRequestDto
    {
        public Guid FieldIdentifier { get; }
        public IFieldValueDto NewValue { get; }

        public FieldUpdateRequestDto(Guid fieldIdentifier, IFieldValueDto newValue)
        {
            FieldIdentifier = fieldIdentifier;
            NewValue = newValue ?? throw new ArgumentNullException(nameof(newValue));
        }
    }
}
