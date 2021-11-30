using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
            NonUniqueAssociatedObject,
            ItemWithIdentifierAlreadyExists,
            ErrorInLine,
            FieldAndError,
        };
        
        private readonly Task _processingTask;
        private int _itemLevelErrorCount = 0;

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
                    Stopwatch stopwatch = new Stopwatch();
                    foreach ((ItemLevelError error, int artifactId) in _queue.GetConsumingEnumerable())
                    {
                        stopwatch.Restart();
                        _itemLevelErrorCount++;
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
                        message = message.Trim();
                        EnsureKeyExists(message);
                        _errorsAggregate[message].Add(artifactId);

                        _logger.LogInformation("Processed log for {artifactId} in {time} ms", artifactId, stopwatch.ElapsedMilliseconds);
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            _queue.CompleteAdding();
            await _processingTask.ConfigureAwait(false);
            stopwatch.Stop();
            
            _logger.LogWarning("Waited {time} ms for log queue to be processed", stopwatch.ElapsedMilliseconds);
            _logger.LogWarning("Total count of item level errors in batch: {count}", _itemLevelErrorCount);
            
            foreach (var keyValuePair in _errorsAggregate)
            {
                _logger.LogWarning("Item level error occured: {message} -> [{items}]", 
                    keyValuePair.Key,
                    string.Join(", ", keyValuePair.Value));
            }
        }
        
        #region Standarization functions
        
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

            return (false, null);
        }

        private static (bool, string) NonUniqueAssociatedObject(string message, ItemLevelError error)
        {
            const string messageTemplate = "IAPI  - A non unique associated object is specified for this new object";
            if (message.StartsWith(messageTemplate))
            {
                return (true, messageTemplate);
            }

            return (false, null);
        }

        private static (bool, string) ItemWithIdentifierAlreadyExists(string message, ItemLevelError error)
        {
            string messageTemplate = "IAPI  - An item with identifier {0} already exists in the workspace";
            if (message.StartsWith(string.Format(messageTemplate, error.Identifier)))
            {
                return (true, messageTemplate);
            }

            return (false, null);
        }

        private static Regex ErrorInLineRegex = new Regex("IAPI Error in line (.*), column"); 
        private static (bool, string) ErrorInLine(string message, ItemLevelError error)
        {
            if (ErrorInLineRegex.IsMatch(message))
            {
                return (true, "IAPI Error in line *, column *.");
            }

            return (false, null);
        }
        
        private static Regex FieldAndErrorRegex = new Regex("IAPI  - Field (.*) Error"); 
        private static (bool, string) FieldAndError(string message, ItemLevelError error)
        {
            if (FieldAndErrorRegex.IsMatch(message))
            {
                return (true, "IAPI Error in line *, column *.");
            }

            return (false, null);
        }

        #endregion
    }
}