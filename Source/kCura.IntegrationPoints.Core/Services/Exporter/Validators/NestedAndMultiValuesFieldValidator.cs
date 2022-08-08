using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Validators
{
    public class NestedAndMultiValuesFieldValidator
    {
        private readonly IAPILog _logger;
        public NestedAndMultiValuesFieldValidator(IAPILog logger)
        {
            _logger = logger.ForContext<NestedAndMultiValuesFieldValidator>();
        }

        public void VerifyObjectField(int fieldArtifactId, string fieldName, ArtifactDTO[] dtos)
        {
            VerifyValidityOfTheNestedOrMultiValuesField(fieldArtifactId, fieldName, dtos, Constants.IntegrationPoints.InvalidMultiObjectsValueFormat);
        }

        public void VerifyChoiceField(int fieldArtifactId, string fieldName, ArtifactDTO[] dtos)
        {
            VerifyValidityOfTheNestedOrMultiValuesField(fieldArtifactId, fieldName, dtos, Constants.IntegrationPoints.InvalidMultiChoicesValueFormat);
        }

        private void VerifyValidityOfTheNestedOrMultiValuesField(int fieldArtifactId, string fieldName, ArtifactDTO[] dtos, Regex invalidPattern)
        {
            var exceptions = new List<Exception>();
            foreach (ArtifactDTO dto in dtos)
            {
                var name = (string)dto.Fields[0].Value;
                if (IsNameInvalid(name, invalidPattern))
                {
                    var exception = new Exception($"Invalid '{fieldName}' : {name}");
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Any())
            {
                LogAndThrowValidationException(fieldArtifactId, fieldName, exceptions);
            }
        }

        private static bool IsNameInvalid(string name, Regex invalidPattern)
        {
            return !string.IsNullOrWhiteSpace(name) && invalidPattern.IsMatch(name);
        }

        private void LogAndThrowValidationException(int fieldArtifactId, string fieldName, List<Exception> exceptions)
        {
            char multiValueDelimiter = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;
            char nestedValueDelimiter = IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER;

            _logger.LogError("Invalid field '{fieldArtifactId}' found. Please remove invalid character(s) - {multiValueDelimiter} or {nestedValueDelimiter}", 
                fieldArtifactId, 
                multiValueDelimiter, 
                nestedValueDelimiter);

            string message = $"Invalid field '{fieldName}' found. Please remove invalid character(s) - {multiValueDelimiter} or {nestedValueDelimiter}";
            var aggregatedException = new AggregateException(exceptions);
            throw new IntegrationPointsException(message, aggregatedException);
        }
    }
}
