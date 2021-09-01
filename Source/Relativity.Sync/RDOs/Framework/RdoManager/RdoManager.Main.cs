using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Relativity.API;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

// ReSharper disable once CheckNamespace
namespace Relativity.Sync.RDOs.Framework
{
    internal partial class RdoManager : IRdoManager
    {
        private readonly ISyncLog _logger;
        private readonly ISyncServiceManager _servicesMgr;
        private readonly IRdoGuidProvider _rdoGuidProvider;

        public RdoManager(ISyncLog logger, ISyncServiceManager servicesMgr, IRdoGuidProvider rdoGuidProvider)
        {
            _logger = logger;
            _servicesMgr = servicesMgr;
            _rdoGuidProvider = rdoGuidProvider;
        }

        public async Task CreateAsync<TRdo>(int workspaceId, TRdo rdo, int? parentObjectId = null) where TRdo : IRdoType
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();

            _logger.LogDebug("Creating RDO object of type {guid} in workspace {workspaceId}", typeInfo.TypeGuid,
                workspaceId);

            CreateRequest request = new CreateRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = typeInfo.TypeGuid
                },
                FieldValues = GetFieldRefValuePairsForSettingValues(rdo, typeInfo.Fields.Values.ToArray())
            };

            if (parentObjectId != null)
            {
                request.ParentObject = new RelativityObjectRef {ArtifactID = parentObjectId.Value};
            }

            using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);

                rdo.ArtifactId = result.Object.ArtifactID;
            }

            _logger.LogInformation(
                "Created RDO object of type {guid} with artifactId {artifactId} in workspace {workspaceId}",
                typeInfo.TypeGuid, rdo.ArtifactId, workspaceId);
        }

        public async Task SetValuesAsync<TRdo>(int workspaceId, TRdo rdo)
            where TRdo : IRdoType
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();

            UpdateRequest request = new UpdateRequest()
            {
                Object = new RelativityObjectRef()
                {
                    ArtifactID = rdo.ArtifactId
                },
                FieldValues = GetFieldRefValuePairsForSettingValues(rdo, typeInfo.Fields.Values.ToArray())
            };

            using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                await objectManager.UpdateAsync(workspaceId, request).ConfigureAwait(false);
                _logger.LogDebug("Set {valuesCount} fields on object {artifactId} in workspace {workspaceId}",
                    typeInfo.Fields.Count, rdo.ArtifactId, workspaceId);
            }
        }

        public async Task SetValueAsync<TRdo, TValue>(int workspaceId, TRdo rdo, Expression<Func<TRdo, TValue>> expression, TValue value) where TRdo : IRdoType
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();
            Guid fieldGuid = _rdoGuidProvider.GetGuidFromFieldExpression(expression);
            RdoFieldInfo fieldInfo = typeInfo.Fields[fieldGuid];

            UpdateRequest request = new UpdateRequest()
            {
                Object = new RelativityObjectRef()
                {
                    ArtifactID = rdo.ArtifactId
                },
                FieldValues = GetFieldRefValuePairsForSettingValue(fieldInfo, value)
            };
            
            using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                await objectManager.UpdateAsync(workspaceId, request).ConfigureAwait(false);
                fieldInfo.PropertyInfo.SetValue(rdo, value);
                
                _logger.LogDebug("Set field {field} on object {artifactId} in workspace {workspaceId}",
                    fieldGuid, rdo.ArtifactId, workspaceId);
            }
        }

       public async Task<TRdo> GetAsync<TRdo>(int workspaceId, int artifactId,
            params Expression<Func<TRdo, object>>[] fields) where TRdo : IRdoType, new()
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();

            _logger.LogDebug(
                "Getting values for RDO of type {guid} with ArtifactId {artifactId} in workspace {workspaceId}",
                typeInfo.TypeGuid, artifactId, workspaceId);

            HashSet<Guid> explicitlyRequestedFieldsGuids = GetFieldsGuidsFromExpressions(fields);

            RdoFieldInfo[] fieldsToQuery = (explicitlyRequestedFieldsGuids.Any()
                    ? explicitlyRequestedFieldsGuids.Select(g => typeInfo.Fields[g])
                    : typeInfo.Fields.Values)
                .ToArray();

            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = typeInfo.TypeGuid
                },
                Fields = fieldsToQuery.Select(x => new FieldRef
                {
                    Guid = x.Guid
                }),
                Condition = $"'ArtifactId' == {artifactId}"
            };

            using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                QueryResultSlim queryResult =
                    await objectManager.QuerySlimAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

                RelativityObjectSlim queryResultObject = queryResult.Objects.FirstOrDefault();


                var result = default(TRdo);

                if (queryResultObject != null)
                {
                    result = new TRdo {ArtifactId = queryResultObject.ArtifactID};

                    foreach (var (fieldInfo, i) in fieldsToQuery.Select((x, i) => (x, i)))
                    {
                        object value = await SanitizeValueAsync(objectManager, queryResultObject.Values[i], fieldInfo,
                                typeInfo.TypeGuid, artifactId, workspaceId)
                            .ConfigureAwait(false);
                        
                        fieldInfo.PropertyInfo.SetValue(result, value);
                    }
                }
                else
                {
                    _logger.LogDebug(
                        "RDO of type {guid} with ArtifactId {artifactId} in workspace {workspaceId} does not exist",
                        typeInfo.TypeGuid, artifactId, workspaceId);
                }

                return result;
            }
        }

        private Task<object> SanitizeValueAsync(IObjectManager objectManager, object value, RdoFieldInfo fieldInfo,
            Guid typeGuid, int objectArtifactId, int workspaceId)
        {
            switch (fieldInfo.Type)
            {
                case RdoFieldType.FixedLengthText
                    when fieldInfo.PropertyInfo.PropertyType == typeof(Guid):
                    return Task.FromResult<object>(new Guid(value.ToString()));

                case RdoFieldType.FixedLengthText
                    when fieldInfo.PropertyInfo.PropertyType == typeof(Guid?):
                    return Task.FromResult(
                        Guid.TryParse(value?.ToString(), out Guid result)
                            ? (object) result
                            : null);
                
                case RdoFieldType.FixedLengthText
                    when fieldInfo.PropertyInfo.PropertyType == typeof(long):
                    return Task.FromResult((object)long.Parse(value.ToString()));
                
                case RdoFieldType.FixedLengthText
                    when fieldInfo.PropertyInfo.PropertyType == typeof(long?):
                    return Task.FromResult(long.TryParse(value.ToString(), out long longValue) 
                        ? (object)longValue 
                        : null);

                case RdoFieldType.LongText when IsTruncatedText(value):
                    return SafeReadLongTextFromStreamAsync(objectManager, fieldInfo, typeGuid, objectArtifactId,
                        workspaceId);
                default:
                    return Task.FromResult(value);
            }
        }

        private Task<object> SafeReadLongTextFromStreamAsync(IObjectManager objectManager, RdoFieldInfo fieldInfo,
            Guid typeGuid, int objectArtifactId, int workspaceId)
        {
            _logger.LogVerbose(
                "Streaming text for field {fieldGuid} of object with ArtifactId {objectArtifactId} in workspace {workspaceId}",
                fieldInfo.Guid, objectArtifactId, workspaceId);
            
            const int maxNumberOfRetries = 2;
            const int maxWaitTime = 500;

            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(maxNumberOfRetries, i => TimeSpan.FromMilliseconds(maxWaitTime))
                .ExecuteAsync(() =>
                    ReadLongTextFieldInternalAsync(objectManager, fieldInfo.Guid, typeGuid, objectArtifactId,
                        workspaceId));
        }

        private async Task<object> ReadLongTextFieldInternalAsync(IObjectManager objectManager, Guid longTextFieldGuid,
            Guid rdoTypeGuid, int objectArtifactId, int workspaceId)
        {
            var exportObject = new RelativityObjectRef
            {
                Guid = rdoTypeGuid,
                ArtifactID = objectArtifactId
            };
            
            var fieldRef = new FieldRef
            {
                Guid = longTextFieldGuid
            };
            
            using (IKeplerStream longTextResult = await objectManager
                .StreamLongTextAsync(workspaceId, exportObject, fieldRef).ConfigureAwait(false))
            using (Stream longTextStream = await longTextResult.GetStreamAsync().ConfigureAwait(false))
            using (var streamReader = new StreamReader(longTextStream, Encoding.Unicode))
            {
                string longTextField = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                return longTextField;
            }
        }

        private bool IsTruncatedText(object value)
        {
            const string longTextTruncateMark = "...";

            string text = value?.ToString();

            return !string.IsNullOrEmpty(text) &&
                   text.EndsWith(longTextTruncateMark, StringComparison.InvariantCulture);
        }

        private IEnumerable<FieldRefValuePair> GetFieldRefValuePairsForSettingValue(RdoFieldInfo fieldInfo, object value)
        {
            return new[]
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        Guid = fieldInfo.Guid
                    },
                    Value = SanitizeValueForSetter(fieldInfo, value)
                }
            };
        }

        private IEnumerable<FieldRefValuePair> GetFieldRefValuePairsForSettingValues<TRdo>(TRdo rdo,
            RdoFieldInfo[] fields)
            where TRdo : IRdoType
        {
            return fields.Select(fieldInfo => new FieldRefValuePair
            {
                Field = new FieldRef
                {
                    Guid = fieldInfo.Guid
                },
                Value = SanitizeValueForSetter(fieldInfo, fieldInfo.PropertyInfo.GetValue(rdo))
            });
        }

        private object SanitizeValueForSetter(RdoFieldInfo fieldInfo, object value)
        {
            switch (fieldInfo.Type)
            {
                case RdoFieldType.FixedLengthText
                    when fieldInfo.PropertyInfo.PropertyType == typeof(long) || fieldInfo.PropertyInfo.PropertyType == typeof(long?):
                    return value?.ToString();
                
                default:
                    return value;
            }
        }

        private HashSet<Guid> GetFieldsGuidsFromExpressions<TRdo>(Expression<Func<TRdo, object>>[] fields)
            where TRdo : IRdoType
        {
            return new HashSet<Guid>(fields.Select(f => _rdoGuidProvider.GetGuidFromFieldExpression(f)));
        }
    }
}