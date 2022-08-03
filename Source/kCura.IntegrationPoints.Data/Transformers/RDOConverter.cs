using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoints.Data.Attributes;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Transformers
{
    public static class RDOConverter
    {
        public static T ToRDO<T>(this RelativityObject source)
            where T : BaseRdo, new()
        {
            var rdo = new T
            {
                ArtifactId = source.ArtifactID
            };
            Type rdoType = typeof(T);

            Dictionary<string, PropertyInfo> propertiesDictionary = GetPropertiesDictionary(rdoType);

            foreach (FieldValuePair item in source.FieldValues)
            {
                string fieldName = item?.Field?.Name;

                if (fieldName == ArtifactFieldNames.TextIdentifier)
                {
                    rdo.RelativityObject.Name = item?.Value as string;
                    continue;
                }

                if (string.IsNullOrEmpty(fieldName) || !propertiesDictionary.ContainsKey(fieldName))
                {
                    throw new IntegrationPointsException(
                        $"Error while converting Relativity object to RDO. Type: '{rdoType}', fieldName: '{fieldName}' ");
                }

                PropertyInfo property = propertiesDictionary[fieldName];
                object valueToSet = ConvertFieldValueToExpectedFormat(item, property.PropertyType);

                property.SetValue(rdo, valueToSet, null);
            }

            return rdo;
        }

        private static object ConvertFieldValueToExpectedFormat(FieldValuePair item, Type propeType)
        {
            object valueToSet = item?.Value;

            if (item?.Value is RelativityObjectValue)
            {
                var itemValue = (RelativityObjectValue)item.Value;
                valueToSet = itemValue.ArtifactID;
            }
            else if (item?.Value is IEnumerable<RelativityObjectValue>)
            {
                var itemValues = (IEnumerable<RelativityObjectValue>)item.Value;
                valueToSet = itemValues.Select(x => x.ArtifactID).ToArray();
            }
            else if (item?.Value is Choice)
            {
                var itemValues = (Choice)item.Value;
                valueToSet = ConvertChoice(itemValues);
            }
            else if (item?.Value is IEnumerable<Choice>)
            {
                var itemValues = (IEnumerable<Choice>)item.Value;
                valueToSet = itemValues.Select(ConvertChoice).ToArray();
            }
            else if (propeType == typeof(int) || propeType == typeof(int?))
            {
                if (item?.Value != null)
                {
                    valueToSet = Convert.ToInt32(item.Value);
                }
            }
            else if (propeType == typeof(long) || propeType == typeof(long?)) 
            {
                if (item?.Value != null)
                {
                    valueToSet = Convert.ToInt64(item.Value);
                }
            }
            else if (item?.Value is DateTime)
            {
                var itemValue = (DateTime)item.Value;
                return DateTime.SpecifyKind(itemValue, DateTimeKind.Utc);
            }

            return valueToSet;
        }

        private static global::Relativity.Services.Choice.ChoiceRef ConvertChoice(Choice choice)
        {
            return new global::Relativity.Services.Choice.ChoiceRef(choice.ArtifactID)
            {
                Guids = choice.Guids,
                Name = choice.Name
            };
        }

        private static ChoiceRef ConvertChoice(global::Relativity.Services.Choice.ChoiceRef choiceRef)
        {
            return new ChoiceRef()
            {
                ArtifactID = choiceRef.ArtifactID,
                Guid = choiceRef.Guids?.FirstOrDefault()
            };
        }
        
        private static Dictionary<string, PropertyInfo> GetPropertiesDictionary(Type rdoType)
        {
            var output = new Dictionary<string, PropertyInfo>();
            foreach (var property in rdoType.GetProperties())
            {
                string fieldName = property?.GetCustomAttribute<DynamicFieldAttribute>()?.FieldName;
                if (property != null)
                {
                    output[property.Name] = property;
                }

                if (!string.IsNullOrEmpty(fieldName))
                {
                    output[fieldName] = property;
                }
            }
            return output;
        }

        private static IEnumerable<FieldRefValuePair> ConvertPropertiesToFieldValuePairs(BaseRdo rdo, BindingFlags bindingAttr)
        {
            PropertyInfo[] properties = rdo.GetType().GetProperties(bindingAttr);
            foreach (PropertyInfo prop in properties)
            {
                DynamicFieldAttribute attributes = prop?.GetCustomAttribute<DynamicFieldAttribute>();
                if (attributes == null)
                {
                    continue;
                }

                FieldRefValuePair output = null;

                if (rdo.HasField(attributes.FieldGuid))
                {
                    output = new FieldRefValuePair
                    {
                        Value = ConvertRdoFieldValueToObjectManagerFieldValue(prop.GetValue(rdo), attributes),
                        Field = new FieldRef { Guid = attributes.FieldGuid, Name = attributes.FieldName }
                    };
                }

                if (output != null)
                {
                    yield return output;
                }
            }
        }

        private static object ConvertRdoFieldValueToObjectManagerFieldValue(object rawValue, DynamicFieldAttribute attributes)
        {
            if (rawValue == null)
            {
                return null;
            }
            if (attributes.Type == FieldTypes.SingleObject)
            {
                return new RelativityObjectValue
                {
                    ArtifactID = (int)rawValue
                };
            }
            if (attributes.Type == FieldTypes.MultipleObject)
            {
                var ids = (IEnumerable<int>)rawValue;
                return ids.Select(x => new RelativityObjectValue { ArtifactID = x }).ToArray();
            }
            if (attributes.Type == FieldTypes.SingleChoice)
            {
                var choice = (global::Relativity.Services.Choice.ChoiceRef)rawValue;
                ChoiceRef convertedChoice = ConvertChoice(choice);
                return convertedChoice;
            }
            if (attributes.Type == FieldTypes.MultipleChoice)
            {
                var choices = (IEnumerable<global::Relativity.Services.Choice.ChoiceRef>)rawValue;
                ChoiceRef[] convertedChoices = choices.Select(ConvertChoice).ToArray();
                return convertedChoices;
            }
            if (attributes.Type == FieldTypes.Date)
            {
                var dateTime = (DateTime)rawValue;
                DateTime dateTimeInUTC = dateTime.ToUniversalTime();
                string dateTimeAsString = dateTimeInUTC.ToString("yyyy-MM-ddTHH:mm:ss.ff");
                return dateTimeAsString;
            }
            return rawValue;

        }

        private static IEnumerable<FieldRef> ConvertPropertiesToFields(BaseRdo rdo, BindingFlags bindingAttr)
        {
            return ConvertPropertiesToFields(rdo.GetType(), bindingAttr);
        }

        public static IEnumerable<FieldRef> ConvertPropertiesToFields<T>() where T : BaseRdo
        {
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
            return ConvertPropertiesToFields(typeof(T), bindingAttr);
        }

        private static IEnumerable<FieldRef> ConvertPropertiesToFields(Type type, BindingFlags bindingAttr)
        {
            foreach (var property in type.GetProperties(bindingAttr))
            {
                DynamicFieldAttribute attribute = property.GetCustomAttribute<DynamicFieldAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                yield return new FieldRef
                {
                    Guid = attribute.FieldGuid,
                    Name = attribute.FieldName
                };
            }
        }

        public static ObjectTypeRef ToObjectType(this BaseRdo rdo)
        {
            DynamicObjectAttribute dynamiCobject = rdo.GetType().GetCustomAttribute<DynamicObjectAttribute>();
            return new ObjectTypeRef() { Guid = Guid.Parse(dynamiCobject.ArtifactTypeGuid) };
        }

        public static RelativityObjectRef ToObjectRef(this BaseRdo rdo)
        {
            return new RelativityObjectRef()
            {
                ArtifactID = rdo.ArtifactId
            };
        }

        public static IEnumerable<FieldRefValuePair> ToFieldValues(this BaseRdo rdo, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            IEnumerable<FieldRefValuePair> fields = ConvertPropertiesToFieldValuePairs(rdo, bindingAttr);

            return fields;
        }

        public static IEnumerable<FieldRef> ToFieldList(this BaseRdo rdo, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            IEnumerable<FieldRef> fields = ConvertPropertiesToFields(rdo, bindingAttr);

            return fields;
        }


    }
}
