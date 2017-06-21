using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Domain.Readers
{
	public abstract class RelativityReaderBase : DataReaderBase
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

		public override int FieldCount
		{
			get { return SchemaDataTable.Columns.Count; }
		}

		public override bool IsClosed { get { return !ReaderOpen; } }

		public override void Close()
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

		public override string GetName(int i)
		{
			return SchemaDataTable.Columns[i].ColumnName;
		}

		public override int GetOrdinal(string name)
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

		public override DataTable GetSchemaTable()
		{
			return SchemaDataTable;
		}

		protected void FetchDataToRead()
		{
			try
			{
				// Request the saved search documents
				FetchedArtifacts = FetchArtifactDTOs();
				if (FetchedArtifacts != null && FetchedArtifacts.Any())
				{
					ReadingArtifactIDs = FetchedArtifacts.Select(artifact => artifact.ArtifactId).ToArray();
					Enumerator = ((IEnumerable<ArtifactDTO>)FetchedArtifacts).GetEnumerator();
					
				}
				else if (!AllArtifactsFetched())
				{
					ReadingArtifactIDs = null;
					Enumerator = null;
				}
				else
				{
					ReaderOpen = false; // TODO: handle errors?
				}
			}
			catch (Exception ex)
			{
				// TODO: Handle errors -- biedrzycki: Jan 13, 2016.
				Dispose();
				throw ex;
			}
		}

		public override bool Read()
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