using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Contracts.Readers
{
	public abstract class RelativityReaderBase : IDataReader
	{
		protected int ReadEntriesCount;
		protected IEnumerator<ArtifactDTO> Enumerator;
		protected bool ReaderOpen;
		protected ArtifactDTO[] FetchedArtifacts;
		protected ArtifactDTO CurrentArtifact;
		protected readonly DataTable SchemaDataTable;
		protected Dictionary<string, int> KnownOrdinalDictionary;
		protected int[] ReadingArtifactIDs;

		protected RelativityReaderBase(DataColumn[] columns)
		{
			KnownOrdinalDictionary = new Dictionary<string, int>();
			SchemaDataTable = new DataTable();
			SchemaDataTable.Columns.AddRange(columns);

			ReaderOpen = true;
		}

		public virtual object this[string name]
		{
			get { return GetValue(GetOrdinal(name)); }
		}

		public virtual object this[int i] { get { return GetValue(i); } }

		public virtual int Depth
		{
			//change if we support nesting in the future
			get { return 0; }
		}

		public int FieldCount
		{
			get { return SchemaDataTable.Columns.Count; }
		}

		public bool IsClosed { get { return !ReaderOpen; } }

		public virtual int RecordsAffected
		{
			// this feature if wanted can be easily added just was not at this point because we are not supporting batching at this point
			get { return -1; }
		}

		public virtual void Close()
		{
			ReaderOpen = false;
			if (Enumerator != null)
			{
				Enumerator.Dispose();
				Enumerator = null;
			}
			CurrentArtifact = null;
			FetchedArtifacts = null;
		}

		public virtual bool GetBoolean(int i)
		{
			return Convert.ToBoolean(GetValue(i));
		}

		public virtual byte GetByte(int i)
		{
			return Convert.ToByte(GetValue(i));
		}

		public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public virtual char GetChar(int i)
		{
			return Convert.ToChar(GetValue(i));
		}

		public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferOffset, int length)
		{
			// We do not need this at this point
			throw new System.NotImplementedException();
		}

		public virtual IDataReader GetData(int i)
		{
			// This is used to expose nested tables and other hierarchical data but currently this is not desired
			throw new System.NotImplementedException();
		}

		public abstract string GetDataTypeName(int i);

		public virtual DateTime GetDateTime(int i)
		{
			return Convert.ToDateTime(GetValue(i));
		}

		public virtual decimal GetDecimal(int i)
		{
			return Convert.ToDecimal(GetValue(i));
		}

		public virtual double GetDouble(int i)
		{
			return Convert.ToDouble(GetValue(i));
		}

		public abstract Type GetFieldType(int i);

		public virtual float GetFloat(int i)
		{
			return Convert.ToSingle(GetValue(i));
		}

		public virtual Guid GetGuid(int i)
		{
			return Guid.Parse(GetString(i));
		}

		public virtual short GetInt16(int i)
		{
			return Convert.ToInt16(GetValue(i));
		}

		public virtual int GetInt32(int i)
		{
			return Convert.ToInt32(GetValue(i));
		}

		public virtual long GetInt64(int i)
		{
			return Convert.ToInt64(GetValue(i));
		}

		public virtual string GetName(int i)
		{
			return SchemaDataTable.Columns[i].ColumnName;
		}

		public virtual int GetOrdinal(string name)
		{
			if (!KnownOrdinalDictionary.ContainsKey(name))
			{
				DataColumn column = SchemaDataTable.Columns[name];
				if (column == null)
				{
					throw new IndexOutOfRangeException(String.Format("'{0}' is not a valid column", name));
				}

				int ordinal = SchemaDataTable.Columns[name].Ordinal;
				KnownOrdinalDictionary[name] = ordinal;
			}
			return KnownOrdinalDictionary[name];
		}

		public virtual DataTable GetSchemaTable()
		{
			return SchemaDataTable;
		}

		public virtual string GetString(int i)
		{
			return Convert.ToString(GetValue(i));
		}

		public abstract object GetValue(int i);

		public virtual int GetValues(object[] values)
		{
			throw new System.NotImplementedException();
		}

		public virtual bool IsDBNull(int i)
		{
			return (GetValue(i) is System.DBNull);
		}

		public virtual bool NextResult()
		{
			return false; // This data reader only ever returns one set of data
		}

		// Following this example: https://msdn.microsoft.com/en-us/library/aa720693(v=vs.71).aspx -- biedrzycki: Jan 20th, 2016
		public void Dispose()
		{
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					this.Close();
				}
				catch (Exception e)
				{
					throw new SystemException("An exception of type " + e.GetType() +
											  " was encountered while closing the " + this.GetType().Name);
				}
			}
		}

		protected void FetchDataToRead()
		{
			try
			{
				// Request the saved search documents
				FetchedArtifacts = FetchArtifactDTOs();
				if (FetchedArtifacts == null || !FetchedArtifacts.Any())
				{
					ReaderOpen = false; // TODO: handle errors?
				}
				else
				{
					ReadingArtifactIDs = FetchedArtifacts.Select(artifact => artifact.ArtifactId).ToArray();
					Enumerator = ((IEnumerable<ArtifactDTO>)FetchedArtifacts).GetEnumerator();
				}
			}
			catch (Exception ex)
			{
				// TODO: Handle errors -- biedrzycki: Jan 13, 2015.
				ReaderOpen = false;
			}
		}

		public virtual bool Read()
		{
			// if the reader is closed, go no further
			if (!ReaderOpen) return false;

			// Check if search results have been populated
			if (Enumerator == null)
			{
				// Request document objects
				FetchDataToRead();
			}

			// Get next result
			if (Enumerator != null && Enumerator.MoveNext())
			{
				CurrentArtifact = Enumerator.Current;
				ReaderOpen = FetchedArtifacts != null;
				ReadEntriesCount++;
			}
			else if (!AllArtifactsFetched())
			{
				FetchDataToRead();
				return Read();
			}
			else
			{
				// No results returned, close the reader
				FetchedArtifacts = null;
				ReaderOpen = false;
			}

			return ReaderOpen;
		}

		protected abstract ArtifactDTO[] FetchArtifactDTOs();

		protected abstract bool AllArtifactsFetched();
	}
}