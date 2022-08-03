using kCura.IntegrationPoints.Data.Repositories.DTO;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Converters
{
    internal static class FieldUpdateRequestDtoExtensions
    {
        public static FieldRefValuePair ToFieldRefValuePair(this FieldUpdateRequestDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new FieldRefValuePair
            {
                Field = new FieldRef
                {
                    Guid = dto.FieldIdentifier
                },
                Value = dto.NewValue.Value
            };
        }
    }
}
