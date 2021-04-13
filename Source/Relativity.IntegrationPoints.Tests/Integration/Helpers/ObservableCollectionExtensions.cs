using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
    public static class ObservableCollectionExtensions
    {
        public static IDisposable SetupOnAddedHandler<T>(this ObservableCollection<T> list, Action<T> onAdd)
        {
            return Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    x => list.CollectionChanged += x,
                    x => list.CollectionChanged -= x)
                .Where(x => x.EventArgs.Action == NotifyCollectionChangedAction.Add)
                .Do(x =>
                {
                    foreach (T newItem in x.EventArgs.NewItems)
                    {
                        onAdd(newItem);
                    }
                })
                .Subscribe();
        }

        public static IDisposable SetupDisposeOnRemove<T>(this ObservableCollection<T> list) where T : IDisposable
        {
            return Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    x => list.CollectionChanged += x,
                    x => list.CollectionChanged -= x)
                .Where(x => x.EventArgs.Action == NotifyCollectionChangedAction.Remove)
                .Do(x =>
                {
                    foreach (IDisposable o in x.EventArgs.OldItems)
                    {
                        o.Dispose();
                    }
                })
                .Subscribe();
        }

        public static void DisposeWith(this IDisposable disposable, CompositeDisposable compositeDisposable)
        {
            compositeDisposable?.Add(disposable);
        }
    }
}