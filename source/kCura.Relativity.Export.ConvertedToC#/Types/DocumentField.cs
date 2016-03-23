using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Relativity;

namespace kCura.Relativity.Export.Types
{
	[Serializable()]
	public class DocumentField : ISerializable
	{

		#region "Members"
		private string _fieldName;
		private Int32 _fieldID;
		private Int32 _fieldTypeID;
		private string _value;
		private Int32 _fieldCategoryID;
		private Nullable<Int32> _codeTypeID;
		private Int32 _fileColumnIndex;
			#endregion
		private Nullable<Int32> _fieldLength;


		#region "Properties"

		[NonSerialized()]
		private Nullable<Int32> _associatedObjectTypeID;
		public Nullable<Int32> AssociatedObjectTypeID {
			get { return _associatedObjectTypeID; }
			set { _associatedObjectTypeID = value; }
		}

		[NonSerialized()]
		private bool _useUnicode;
		public bool UseUnicode {
			get { return _useUnicode; }
			set { _useUnicode = value; }
		}

		[NonSerialized()]
		private bool _enableDataGrid;
		public bool EnableDataGrid {
			get { return _enableDataGrid; }
			set { _enableDataGrid = value; }
		}

		public string FieldName {
			get { return _fieldName; }
			set { _fieldName = value; }
		}

		public Int32 FieldID {
			get { return _fieldID; }
			set { _fieldID = value; }
		}

		public Int32 FieldTypeID {
			get { return _fieldTypeID; }
			set { _fieldTypeID = value; }
		}

		public Int32 FieldCategoryID {
			get { return _fieldCategoryID; }
			set { _fieldCategoryID = value; }
		}

		public FieldCategory FieldCategory {
			get { return (FieldCategory)_fieldCategoryID; }
			set { _fieldCategoryID = (int)value; }
		}

		public string Value {
			get { return _value; }
			set { _value = value; }
		}

		public Nullable<Int32> CodeTypeID {
			get { return _codeTypeID; }
			set { _codeTypeID = value; }
		}

		public Int32 FileColumnIndex {
			get { return _fileColumnIndex; }
			set { _fileColumnIndex = value; }
		}

		public Nullable<Int32> FieldLength {
			get { return _fieldLength; }
			set { _fieldLength = value; }
		}

		public List<Guid> Guids { get; set; }

		#endregion

		#region "Constructors"

		private DocumentField(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext Context)
		{
			this.FieldName = info.GetString("_fieldName");
			this.FieldID = info.GetInt32("_fieldID");
			this.FieldTypeID = info.GetInt32("_fieldTypeID");
			this.Value = info.GetString("_value");
			this.FieldCategoryID = info.GetInt32("_fieldCategoryID");
			this.FileColumnIndex = info.GetInt32("_fileColumnIndex");
			this.Guids = new List<Guid>();
		}

		public DocumentField(string fieldName, Int32 fieldID, Int32 fieldTypeID, Int32 fieldCategoryID, Nullable<Int32> codeTypeID, Nullable<Int32> fieldLength, Nullable<Int32> associatedObjectTypeID, bool useUnicode, bool enableDataGrid) : this(fieldName, fieldID, fieldTypeID, fieldCategoryID, codeTypeID, fieldLength, associatedObjectTypeID, useUnicode, new List<Guid>(), enableDataGrid)
		{
		}

		public DocumentField(string fieldName, Int32 fieldID, Int32 fieldTypeID, Int32 fieldCategoryID, Nullable<Int32> codeTypeID, Nullable<Int32> fieldLength, Nullable<Int32> associatedObjectTypeID, bool useUnicode, IEnumerable<Guid> guids, bool enableDataGrid) : base()
		{
			_fieldName = fieldName;
			_fieldID = fieldID;
			_fieldTypeID = fieldTypeID;
			_fieldCategoryID = fieldCategoryID;
			_codeTypeID = codeTypeID;
			_fieldLength = fieldLength;
			_associatedObjectTypeID = associatedObjectTypeID;
			_useUnicode = useUnicode;
			_enableDataGrid = enableDataGrid;
			if (((guids != null))) {
				this.Guids = guids.ToList();
			} else {
				this.Guids = new List<Guid>();
			}
		}

		public DocumentField(DocumentField docField) : this(docField.FieldName, docField.FieldID, docField.FieldTypeID, docField.FieldCategoryID, docField.CodeTypeID, docField.FieldLength, docField.AssociatedObjectTypeID, docField.UseUnicode, docField.EnableDataGrid)
		{
		}

		#endregion

		public string ToDisplayString()
		{
			return string.Format("DocumentField[{0},{1},{2},{3},'{4}']", FieldCategoryID, FieldID, FieldName, FieldTypeID, kCura.Utility.NullableTypesHelper.ToEmptyStringOrValue(CodeTypeID));
		}

		public override string ToString()
		{
			return FieldName;
		}

		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("_fieldName", this.FieldName, typeof(string));
			info.AddValue("_fieldID", this.FieldID, typeof(Int32));
			info.AddValue("_fieldTypeID", this.FieldTypeID, typeof(Int32));
			info.AddValue("_value", this.Value, typeof(string));
			info.AddValue("_fieldCategoryID", this.FieldCategoryID, typeof(Int32));
			info.AddValue("_fileColumnIndex", this.FileColumnIndex, typeof(Int32));
		}

		public static bool op_Equality(DocumentField df1, DocumentField df2)
		{
			bool areEqual = false;
			if (df1.CodeTypeID == null) {
				if (df2.CodeTypeID == null) {
					areEqual = true;
				} else {
					areEqual = false;
				}
			} else {
				if (df2.CodeTypeID == null) {
					areEqual = true;
				} else {
					areEqual = df1.CodeTypeID.Value == df2.CodeTypeID.Value;
				}
			}
			areEqual = areEqual & df1.FieldCategoryID == df2.FieldCategoryID;
			areEqual = areEqual & df1.FieldName == df2.FieldName;
			areEqual = areEqual & df1.FieldTypeID == df2.FieldTypeID;
			areEqual = areEqual & df1.Value == df2.Value;
			return areEqual;
		}

	}

}
