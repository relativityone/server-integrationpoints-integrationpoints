using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using kCura.IntegrationPoints.Data.Attributes;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Data
{
    public abstract class BaseRdo : IBaseRdo
    {
        private RelativityObject _relativityObject;
        
        internal RelativityObject RelativityObject
        {
            get
            {
                if (_relativityObject == null)
                {
                    _relativityObject = new RelativityObject()
                    {
                        Guids = new List<Guid>(),
                        FieldValues = new List<FieldValuePair>()
                    };
                }

                return _relativityObject;
            }
            set
            {
                _relativityObject = value;
            }
        }

        public virtual bool HasField(Guid fieldGuid)
        {
            return RelativityObject.FieldValuePairExists(fieldGuid);
        }

        public virtual T GetField<T>(Guid fieldGuid)
        {
            string fieldType = FieldMetadata[fieldGuid].Type;
            object fieldValue = RelativityObject[fieldGuid].Value;
            object convertedValue = ConvertForGet(fieldType, fieldValue);
            return (T)convertedValue;
        }

        public string GetFieldName(Guid fieldGuid)
        {
            return FieldMetadata.Single(x => x.Value.FieldGuid == fieldGuid).Value.FieldName;
        }

        public virtual void SetField<T>(Guid fieldGuid, T fieldValue, bool markAsUpdated = true)
        {
            object value = ConvertValue(FieldMetadata[fieldGuid].Type, fieldValue);
            if (!RelativityObject.FieldValuePairExists(fieldGuid))
            {
                RelativityObject.FieldValues.Add(new FieldValuePair()
                {
                    Field = new Field()
                    {
                        Guids = new List<Guid>()
                        {
                            fieldGuid
                        }
                    },
                    Value = value
                });
            }
            else
            {
                RelativityObject[fieldGuid].Value = value;
            }
        }

        internal object ConvertForGet(string fieldType, object value)
        {
            switch (fieldType)
            {
                case FieldTypes.MultipleObject:
                    if (value is IEnumerable<RelativityObject>)
                    {
                        return ((IEnumerable<RelativityObject>)value).Select(x => x.ArtifactID).ToArray();
                    }
                    return new int[] { };
                case FieldTypes.SingleObject:
                    var singleObjectValue = value as RelativityObject;
                    if (singleObjectValue != null)
                    {
                        return singleObjectValue.ArtifactID;
                    }
                    return value;
                case FieldTypes.SingleChoice:
                    return value as ChoiceRef;
                default:
                    return value;
            }

        }

        internal object ConvertValue(string fieldType, object value)
        {
            if (value == null)
            {
                return value;
            }
            object newValue = null;

            switch (fieldType)
            {
                case FieldTypes.MultipleChoice:
                    ChoiceRef[] choices = null;
                    if (value is List<ChoiceRef>)
                    {
                        choices = ((List<ChoiceRef>)value).ToArray();
                    }
                    else if (value is object[])
                    {
                        choices = ((object[])value).Select(x => ((ChoiceRef)x)).ToArray();
                    }
                    newValue = choices;
                    break;
                case FieldTypes.SingleChoice:
                    ChoiceRef singleChoice = null;
                    if (value is ChoiceRef)
                    {
                        singleChoice = (ChoiceRef)value;

                        if (singleChoice.ArtifactID > 0 || singleChoice.Guids.Any())
                        {
                            newValue = new ChoiceRef(singleChoice.ArtifactID)
                            {
                                Name = singleChoice.Name,
                                Guids = singleChoice.Guids
                            };
                        }
                        else
                        {
                            throw new Exception("Can not determine choice with no artifact id or guid.");
                        }
                    }
                    break;
                case FieldTypes.SingleObject:
                    if (value is int)
                    {
                        RelativityObject relativityObject = new RelativityObject()
                        {
                            ArtifactID = (int) value
                        };
                        newValue = relativityObject;
                    }
                    break;
                case FieldTypes.MultipleObject:
                    int[] multipleObjectIDs;
                    if (value is int[])
                    {
                        multipleObjectIDs = (int[])value;
                        newValue = multipleObjectIDs.Select(x => new RelativityObject()
                        {
                            ArtifactID = x
                        }).ToArray();
                    }
                    break;
                default:
                    newValue = value;
                    break;
            }
            return newValue;
        }

        public static Dictionary<Guid, DynamicFieldAttribute> GetFieldMetadata(Type type)
        {
            IEnumerable<DynamicFieldAttribute> dynamicFieldAttributes = type
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetCustomAttributes(typeof(DynamicFieldAttribute), inherit: true))
                .Where(attributes => attributes.Any())
                .Select(attributes => (DynamicFieldAttribute) attributes.First());

            Dictionary<Guid, DynamicFieldAttribute> dynamicFieldAttributesDictionary = dynamicFieldAttributes
                .ToDictionary(attribute => attribute.FieldGuid);

            return dynamicFieldAttributesDictionary;
        }
        
        public static Guid GetFieldGuid<TRdo, TProperty>(Expression<Func<TRdo, TProperty>> propertySelector)
            where TRdo : BaseRdo
        {
            Guid fieldGuid = GetPropertyInfo(propertySelector)
                .GetCustomAttributes(typeof(DynamicFieldAttribute), inherit:true)
                .Cast<DynamicFieldAttribute>()
                .Single()
                .FieldGuid;

            return fieldGuid;
        }

        private static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            if (!(propertyLambda.Body is MemberExpression member))
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
            }

            if (propInfo.ReflectedType != null && type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a property that is not from type {type}.");
            }

            return propInfo;
        }

        public int ArtifactId
        {
            get
            {
                return RelativityObject.ArtifactID;
            }
            set
            {
                RelativityObject.ArtifactID = value;
            }
        }

        public int? ParentArtifactId
        {
            get
            {
                if (RelativityObject.ParentObject != null)
                {
                    return RelativityObject.ParentObject.ArtifactID;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    RelativityObject.ParentObject = new RelativityObjectRef()
                    {
                        ArtifactID = value.Value
                    };
                }
            }
        }
        public abstract Dictionary<Guid, DynamicFieldAttribute> FieldMetadata { get; }
    }
}
