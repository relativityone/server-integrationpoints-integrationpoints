using System;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Storage
{
    internal static class UpdateRequestExtensions
    {
        public static UpdateRequest CreateForSingleField<T>(int artifactId, Guid fieldGuid, T value)
        {
            return new UpdateRequest
            {
                Object = new RelativityObjectRef
                {
                    ArtifactID = artifactId
                },
                FieldValues = new[]
                {
                    new FieldRefValuePair
                    {
                        Field = new FieldRef
                        {
                            Guid = fieldGuid
                        },
                        Value = value
                    }
                }
            };
        }
    }
}