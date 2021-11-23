using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Logging
{
    internal class ItemLevelErrorLogAggregator : IItemLevelErrorLogAggregator
    {
        private readonly ISyncLog _logger;
        private readonly Dictionary<string, List<int>> _errorsAggregate;
        private readonly BlockingCollection<(ItemLevelError, int)> _queue = new BlockingCollection<(ItemLevelError, int)>();
        private readonly Func<string, ItemLevelError, (bool matched, string newMessage)>[] _standarizationFunctions = {
            ReplaceIdentifier,
            FailedToCopySourceField,
            TrimWhitespace
        };
        
        private readonly Task _processingTask;

        public ItemLevelErrorLogAggregator(ISyncLog logger)
        {
            _logger = logger;
            _errorsAggregate = new Dictionary<string, List<int>>();

            _processingTask = StartProcessing();
        }

        private Task StartProcessing()
        {
            return Task.Run(() =>
            {
                try
                {
                    foreach ((ItemLevelError error, int artifactId) in _queue.GetConsumingEnumerable())
                    {
                        string message = error.Message;
                        foreach (var f in _standarizationFunctions)
                        {
                            (bool matched, string newMessage) = f(message, error);
                            if (matched)
                            {
                                message = newMessage;
                                break;
                            }
                        }

                        // no lock needed since there is only one thread doing this
                        EnsureKeyExists(message);
                        _errorsAggregate[message].Add(artifactId);

                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error when aggregating item level errors");
                }
            });
        }

        public void AddItemLevelError(ItemLevelError itemLevelError, int artifactId)
        {
            if (!_queue.IsAddingCompleted)
            {
                _queue.Add((itemLevelError, artifactId));
            }
        }

        private void EnsureKeyExists(string messageTemplate)
        {
            if (!_errorsAggregate.ContainsKey(messageTemplate))
            {
                _errorsAggregate.Add(messageTemplate, new List<int>());
            }
        }

        public async Task LogAllItemLevelErrorsAsync()
        {
            _queue.CompleteAdding();
            await _processingTask.ConfigureAwait(false);
            
            foreach (var keyValuePair in _errorsAggregate)
            {
                _logger.LogWarning("Item level error occured: {message} -> [{items}]", 
                    keyValuePair.Key,
                    string.Join(", ", keyValuePair.Value));
            }
        }
        
        #region Standarization functions

        private static (bool matched, string newMessage) TrimWhitespace(string message, ItemLevelError error)
        {
            return (true, message.Trim());
        }

        private static (bool matched, string newMessage) ReplaceIdentifier(string message, ItemLevelError error)
        {
            return (false, message.Replace(error.Identifier, "[identifier]"));
        }

        private static (bool matched, string newMessage) FailedToCopySourceField(string message, ItemLevelError _)
        {
            const string resultMessage =
                "IAPI  - 20.006. Failed to copy source field into destination field due to missing child object";
            
            const string messageTemplate =
                resultMessage + ". Review the following destination field(s):";
            
            if (message.StartsWith(messageTemplate))
            {
                return (true, resultMessage);
            }

            return (false,null);
        }
        
        #endregion
    }
}