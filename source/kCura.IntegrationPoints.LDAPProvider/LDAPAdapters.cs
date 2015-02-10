using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;

namespace LDAPProvider
{
	public class LDAPAdapters
	{
		public interface IDirectoryEntryAdapter
		{
			IDictionary Properties { get; } //of IList
			void CommitChanges();
			string GetDomain();
			string GetManager();
		}

		public interface IDirectorySearcherAdapter
		{
			string Filter { get; set; }
			SearchScope SearchScope { get; set; }
			ISearchResultAdapter FindOne();
			DirectoryEntry GetDirectoryEntry();
		}

		public interface IResultPropertyValueCollectionAdapter
		{
			object this[int index] { get; }
			IEnumerator GetEnumerator();
			int Count { get; }
		}

		public interface ISearchResultAdapter
		{
			IDirectoryEntryAdapter GetDirectoryEntry();
			IDictionary Properties { get; } //of IResultPropertyValueCollectionAdapter
		}

		//concretes

		public class DirectoryEntryAdapter : IDirectoryEntryAdapter
		{
			public DirectoryEntryAdapter()
			{
				//used for unit tests
			}

			public DirectoryEntryAdapter(DirectoryEntry entry)
			{
				_entry = entry;
			}

			private DirectoryEntry _entry;

			public IDictionary Properties //of IList
			{
				get { return _entry.Properties; }
			}

			public void CommitChanges()
			{
				_entry.CommitChanges();
			}

			public virtual string GetDomain()
			{
				return GetDomain(_entry).ToString();
			}

			public virtual string GetManager()
			{
				object manager = null;
				try { manager = _entry.Properties["manager"].Value; }
				catch
				{ }
				return manager == null ? string.Empty : manager.ToString();
			}

			private static object GetDomain(DirectoryEntry dEntry)
			{
				if (dEntry == null) return string.Empty;

				if (dEntry.SchemaClassName == "domainDNS")
				{
					object domain = null;
					try { domain = dEntry.Properties["dc"].Value; }
					catch
					{ }
					if (domain == null)
					{
						try { domain = dEntry.Properties["name"].Value; }
						catch
						{ }
					}
					return domain;
				}
				else
				{
					return GetDomain(dEntry.Parent);
				}
			}
		}

		public class DirectorySearcherAdapter : IDirectorySearcherAdapter, System.IDisposable
		{
			public DirectorySearcherAdapter(DirectorySearcher searcher)
			{
				_searcher = searcher;
			}

			public string Filter { get { return _searcher.Filter; } set { _searcher.Filter = value; } }
			public SearchScope SearchScope { get { return _searcher.SearchScope; } set { _searcher.SearchScope = value; } }

			public ISearchResultAdapter FindOne() { return new SearchResultAdapter(_searcher.FindOne()); }
			public DirectoryEntry GetDirectoryEntry() { return _searcher.SearchRoot; }

			private DirectorySearcher _searcher;

			public void Dispose()
			{
				_searcher.Dispose();
			}
		}

		public class ResultPropertyValueCollectionAdapter : IResultPropertyValueCollectionAdapter, IEnumerable
		{
			public ResultPropertyValueCollectionAdapter(ResultPropertyValueCollection input)
			{
				_collection = input;
			}

			public object this[int index]
			{
				get { return _collection[index]; }
				//set { _collection[index] = value.ToString(); }
			}

			public IEnumerator GetEnumerator()
			{
				return _collection.GetEnumerator();
			}

			public int Count { get { return _collection.Count; } }

			internal static IDictionary<int, object> ToDictionary(ResultPropertyValueCollection _collection)
			{
				IDictionary<int, object> dictionaryObject = new Dictionary<int, object>();

				for (int i = 0; i < _collection.Count; i++)
				{
					dictionaryObject.Add(i, _collection[i]);
				}
				return dictionaryObject;
			}

			private ResultPropertyValueCollection _collection;
		}

		public class SearchResultAdapter : ISearchResultAdapter
		{
			public SearchResultAdapter()
			{
				//used only for unit tests
			}
			public SearchResultAdapter(SearchResult searchResult)
			{
				_searchResult = searchResult;
			}

			public virtual IDictionary Properties //of IResultPropertyValueCollectionAdapter
			{
				get
				{
					if (_properties == null)
					{
						_properties = new Dictionary<string, ArrayList>();

						foreach (string key in _searchResult.Properties.PropertyNames)
						{
							var newItem = new ResultPropertyValueCollectionAdapter(_searchResult.Properties[key]);
							ArrayList items = new ArrayList();
							for (int i = 0; i < newItem.Count; i++)
							{
								items.Add(newItem[i]);
							}
							_properties.Add(key, items);
						}
					}
					return _properties;
				}
			}

			public virtual IDirectoryEntryAdapter GetDirectoryEntry()
			{
				return new DirectoryEntryAdapter(_searchResult.GetDirectoryEntry());
			}

			private SearchResult _searchResult;
			private Dictionary<string, ArrayList> _properties;
		}

	}

}
