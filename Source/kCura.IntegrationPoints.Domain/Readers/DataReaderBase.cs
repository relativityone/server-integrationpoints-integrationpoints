using System;
using System.Data;

namespace kCura.IntegrationPoints.Domain.Readers
{
    public abstract class DataReaderBase : IDataReader
    {
        private bool _isDisposed = false; 
        //Abstract Properties

        public abstract int FieldCount { get; }
        public abstract bool IsClosed { get; }
        public abstract Type GetFieldType(int i);

        //Abstract Methods 

        public abstract void Close();
        public abstract string GetName(int i);
        public abstract int GetOrdinal(string name);
        public abstract DataTable GetSchemaTable();
        public abstract object GetValue(int i);
        public abstract bool Read();
        public abstract string GetDataTypeName(int i);

        //IDataReader implementation

        public virtual object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public virtual object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        public virtual int Depth
        {
            //change if we support nesting in the future
            get
            {
                return 0;
            }
        }

        public virtual int RecordsAffected
        {
            // this feature if wanted can be easily added just was not at this point because we are not supporting batching at this point
            get
            {
                return -1;
            }
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

        public virtual string GetString(int i)
        {
            return Convert.ToString(GetValue(i));
        }

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
            if (_isDisposed)
            {
                return;
            }

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
                _isDisposed = true;
            }
        }

    }
}
