using System;
using System.Collections.Generic;
using kCura.EDDS.WebAPI.FieldManagerBase;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using Moq;
using NUnit.Framework;
using Relativity.API.Foundation;
using FieldCategory = Relativity.API.Foundation.FieldCategory;
using FieldType = Relativity.API.Foundation.FieldType;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Helpers
{
	[TestFixture]
	public class FieldConverterTests
	{
		private const bool _ALLOW_GROUP_BY = true;
		private const bool _ALLOW_HTML = true;
		private const bool _ALLOW_PIVOT = true;
		private const bool _AUTO_ADD_CHOICES = true;
		private const bool _AVAILABLE_IN_VIEWER = true;
		private const bool _DELETE_FLAG = true;
		private const bool _ENABLE_DATA_GRID = true;
		private const bool _IS_ARTIFACT_BASE_FIELD = true;
		private const bool _IS_AVAILABLE_IN_CHOICE_TREE = true;
		private const bool _IS_AVAILABLE_TO_ASSOCIATIVE_OBJECTS = true;
		private const bool _IS_EDITABLE = true;
		private const bool _IS_GROUP_BY_ENABLED = true;
		private const bool _IS_INDEX_ENABLED = true;
		private const bool _IS_LINKED = true;
		private const bool _IS_REMOVABLE = true;
		private const bool _IS_REQUIRED = true;
		private const bool _IS_SORTABLE = true;
		private const bool _IS_VISIBLE = true;
		private const bool _KEYBOARD_SHORTCUT_ALT = false;
		private const bool _KEYBOARD_SHORTCUT_CTRL = false;
		private const bool _KEYBOARD_SHORTCUT_SHIFT = true;
		private const bool _USE_UNICODE_ENCODING = true;
		private const bool _WRAPPING = true;
		private const EDDS.WebAPI.FieldManagerBase.FieldCategory _FIELD_CATEGORY_CONVERTED = EDDS.WebAPI.FieldManagerBase.FieldCategory.Batch;
		private const EDDS.WebAPI.FieldManagerBase.FieldType _FIELD_TYPE_CONVERTED = EDDS.WebAPI.FieldManagerBase.FieldType.Code;
		private const FieldCategory _FIELD_CATEGORY = FieldCategory.Batch;
		private const FieldType _FIELD_TYPE = FieldType.Code;
		private const int _ARTIFACT_ID = 1017415;
		private const int _ARTIFACT_TYPE_ID = 10;
		private const int _CREATED_BY = 1049713;
		private const int _FIELD_ARTIFACT_TYPE_ID = 11;
		private const int _FIELD_DISPLAY_TYPE_ID = 12;
		private const int _KEYBOARD_SHORTCUT_ID = 1048234;
		private const int _KEYBOARD_SHORTCUT_KEY = 14;
		private const int _LAST_MODIFIED_BY = 1059812;
		private const int _LINK_LAYOUT_ARTIFACT_ID = 1060123;
		private const int _MAX_LENGTH = 15;
		private const int _VIEW_FIELD_ID = 1027594;
		private const string _COLUMN_NAME = "ControlNumber";
		private const string _DISPLAY_VALUE_FALSE = "False";
		private const string _DISPLAY_VALUE_TRUE = "True";
		private const string _FILTER_TYPE = "Text";
		private const string _FORMAT_STRING = "{}";
		private const string _FRIENDLY_NAME = "Control Number";
		private const string _KEYWORDS = "Keywords";
		private const string _NAME = "Control Number Name";
		private const string _NOTES = "Notes";
		private const string _TEXT_IDENTIFIER = "TextIdentifier";
		private const string _VALUE = "Value";
		private const string _WIDTH = "17";

		private readonly bool? _overlayBehavior = true;
		private readonly DateTime _createdOn = new DateTime(2019, 2, 1);
		private readonly DateTime _lastModifiedOn = new DateTime(2019, 2, 2);
		private readonly FieldImportBehavior? _IMPORT_BEHAVIOR = FieldImportBehavior.ObjectFieldContainsArtifactId;
		private readonly IList<Guid> _guids = new List<Guid>
		{
			Guid.Parse("4D8763F2-1EF2-478F-8AB7-0A25E902DACF"),
			Guid.Parse("448D35D6-552A-4C14-B59D-0F3ABCF1BFF2")
		};
		private readonly ImportBehaviorChoice? _IMPORT_BEHAVIOR_CONVERTED = ImportBehaviorChoice.ObjectFieldContainsArtifactId;
		private readonly int? _associativeArtifactTypeID = 1031234;
		private readonly int? _codeTypeID = 11;
		private readonly int? _fieldTreeView = 13;
		private readonly int? _parentArtifactID = 1074747;
		private readonly int? _popupPickerView = 16;
		private readonly int? _relationalIndexViewArtifactID = 1084387;
		private readonly KeyboardShortcut _keyboardShortcutConverted = CreateKeyboardShortcutConverted();
		private readonly Mock<IKeyboardShortcut> _keyboardShortcut = CreateKeyboardShortcutMock();

		[Test]
		public void ConvertToFieldTest()
		{
			// Arrange
			IField field = CreateFieldMock().Object;
			var converter = new FieldConverter();

			// Act
			Field result = converter.ConvertToField(field);

			// Assert
			ValidateConvertedFieldValues(result);
		}

		private static Mock<IKeyboardShortcut> CreateKeyboardShortcutMock()
		{
			var keyboardShortcutMock = new Mock<IKeyboardShortcut>();
			keyboardShortcutMock.Setup(x => x.ID).Returns(_KEYBOARD_SHORTCUT_ID);
			keyboardShortcutMock.Setup(x => x.Shift).Returns(_KEYBOARD_SHORTCUT_SHIFT);
			keyboardShortcutMock.Setup(x => x.Ctrl).Returns(_KEYBOARD_SHORTCUT_CTRL);
			keyboardShortcutMock.Setup(x => x.Alt).Returns(_KEYBOARD_SHORTCUT_ALT);
			keyboardShortcutMock.Setup(x => x.Key).Returns(_KEYBOARD_SHORTCUT_KEY);
			return keyboardShortcutMock;
		}

		private static KeyboardShortcut CreateKeyboardShortcutConverted()
		{
			return new KeyboardShortcut
			{
				Alt = _KEYBOARD_SHORTCUT_ALT,
				Ctrl = _KEYBOARD_SHORTCUT_CTRL,
				Id = _KEYBOARD_SHORTCUT_ID,
				Key = _KEYBOARD_SHORTCUT_KEY,
				Shift = _KEYBOARD_SHORTCUT_SHIFT
			};
		}

		private Mock<IField> CreateFieldMock()
		{
			var fieldMock = new Mock<IField>();
			fieldMock.Setup(x => x.AllowGroupBy).Returns(_ALLOW_GROUP_BY);
			fieldMock.Setup(x => x.AllowHtml).Returns(_ALLOW_HTML);
			fieldMock.Setup(x => x.AllowPivot).Returns(_ALLOW_PIVOT);
			fieldMock.Setup(x => x.ArtifactID).Returns(_ARTIFACT_ID);
			fieldMock.Setup(x => x.ArtifactTypeID).Returns(_ARTIFACT_TYPE_ID);
			fieldMock.Setup(x => x.ViewFieldID).Returns(_VIEW_FIELD_ID);
			fieldMock.Setup(x => x.AssociativeArtifactTypeID).Returns(_associativeArtifactTypeID);
			fieldMock.Setup(x => x.AutoAddChoices).Returns(_AUTO_ADD_CHOICES);
			fieldMock.Setup(x => x.AvailableInViewer).Returns(_AVAILABLE_IN_VIEWER);
			fieldMock.Setup(x => x.CodeTypeID).Returns(_codeTypeID);
			fieldMock.Setup(x => x.ColumnName).Returns(_COLUMN_NAME);
			fieldMock.Setup(x => x.CreatedBy).Returns(_CREATED_BY);
			fieldMock.Setup(x => x.CreatedOn).Returns(_createdOn);
			fieldMock.Setup(x => x.DeleteFlag).Returns(_DELETE_FLAG);
			fieldMock.Setup(x => x.DisplayValueFalse).Returns(_DISPLAY_VALUE_FALSE);
			fieldMock.Setup(x => x.DisplayValueTrue).Returns(_DISPLAY_VALUE_TRUE);
			fieldMock.Setup(x => x.EnableDataGrid).Returns(_ENABLE_DATA_GRID);
			fieldMock.Setup(x => x.FieldArtifactTypeID).Returns(_FIELD_ARTIFACT_TYPE_ID);
			fieldMock.Setup(x => x.FieldCategory).Returns(_FIELD_CATEGORY);
			fieldMock.Setup(x => x.FieldDisplayTypeID).Returns(_FIELD_DISPLAY_TYPE_ID);
			fieldMock.Setup(x => x.FieldTreeView).Returns(_fieldTreeView);
			fieldMock.Setup(x => x.FieldType).Returns(_FIELD_TYPE);
			fieldMock.Setup(x => x.FilterType).Returns(_FILTER_TYPE);
			fieldMock.Setup(x => x.FormatString).Returns(_FORMAT_STRING);
			fieldMock.Setup(x => x.FriendlyName).Returns(_FRIENDLY_NAME);
			fieldMock.Setup(x => x.Guids).Returns(_guids);
			fieldMock.Setup(x => x.ImportBehavior).Returns(_IMPORT_BEHAVIOR);
			fieldMock.Setup(x => x.IsArtifactBaseField).Returns(_IS_ARTIFACT_BASE_FIELD);
			fieldMock.Setup(x => x.IsAvailableInChoiceTree).Returns(_IS_AVAILABLE_IN_CHOICE_TREE);
			fieldMock.Setup(x => x.IsAvailableToAssociativeObjects).Returns(_IS_AVAILABLE_TO_ASSOCIATIVE_OBJECTS);
			fieldMock.Setup(x => x.IsEditable).Returns(_IS_EDITABLE);
			fieldMock.Setup(x => x.IsGroupByEnabled).Returns(_IS_GROUP_BY_ENABLED);
			fieldMock.Setup(x => x.IsIndexEnabled).Returns(_IS_INDEX_ENABLED);
			fieldMock.Setup(x => x.IsLinked).Returns(_IS_LINKED);
			fieldMock.Setup(x => x.IsRemovable).Returns(_IS_REMOVABLE);
			fieldMock.Setup(x => x.IsRequired).Returns(_IS_REQUIRED);
			fieldMock.Setup(x => x.IsSortable).Returns(_IS_SORTABLE);
			fieldMock.Setup(x => x.IsVisible).Returns(_IS_VISIBLE);
			fieldMock.Setup(x => x.KeyboardShortcut).Returns(_keyboardShortcut.Object);
			fieldMock.Setup(x => x.Keywords).Returns(_KEYWORDS);
			fieldMock.Setup(x => x.LastModifiedBy).Returns(_LAST_MODIFIED_BY);
			fieldMock.Setup(x => x.LastModifiedOn).Returns(_lastModifiedOn);
			fieldMock.Setup(x => x.LinkLayoutArtifactID).Returns(_LINK_LAYOUT_ARTIFACT_ID);
			fieldMock.Setup(x => x.MaxLength).Returns(_MAX_LENGTH);
			fieldMock.Setup(x => x.Name).Returns(_NAME);
			fieldMock.Setup(x => x.Notes).Returns(_NOTES);
			fieldMock.Setup(x => x.OverlayBehavior).Returns(_overlayBehavior);
			fieldMock.Setup(x => x.ParentArtifactID).Returns(_parentArtifactID);
			fieldMock.Setup(x => x.PopupPickerView).Returns(_popupPickerView);
			fieldMock.Setup(x => x.RelationalIndexViewArtifactID).Returns(_relationalIndexViewArtifactID);
			fieldMock.Setup(x => x.TextIdentifier).Returns(_TEXT_IDENTIFIER);
			fieldMock.Setup(x => x.UseUnicodeEncoding).Returns(_USE_UNICODE_ENCODING);
			fieldMock.Setup(x => x.Value).Returns(_VALUE);
			fieldMock.Setup(x => x.Width).Returns(_WIDTH);
			fieldMock.Setup(x => x.Wrapping).Returns(_WRAPPING);
			return fieldMock;
		}

		private void ValidateConvertedFieldValues(Field field)
		{
			Assert.AreEqual(_ALLOW_GROUP_BY, field.AllowGroupBy);
			Assert.AreEqual(_ALLOW_HTML, field.AllowHtml);
			Assert.AreEqual(_ALLOW_PIVOT, field.AllowPivot);
			Assert.AreEqual(_ARTIFACT_ID, field.ArtifactID);
			Assert.AreEqual(_ARTIFACT_TYPE_ID, field.ArtifactTypeID);
			Assert.AreEqual(_VIEW_FIELD_ID, field.ArtifactViewFieldID);
			Assert.AreEqual(_associativeArtifactTypeID, field.AssociativeArtifactTypeID);
			Assert.AreEqual(_AUTO_ADD_CHOICES, field.AutoAddChoices);
			Assert.AreEqual(_AVAILABLE_IN_VIEWER, field.AvailableInViewer);
			Assert.AreEqual(_codeTypeID, field.CodeTypeID);
			Assert.AreEqual(_COLUMN_NAME, field.ColumnName);
			Assert.AreEqual(_CREATED_BY, field.CreatedBy);
			Assert.AreEqual(_createdOn, field.CreatedOn);
			Assert.AreEqual(_DELETE_FLAG, field.DeleteFlag);
			Assert.AreEqual(_DISPLAY_VALUE_FALSE, field.DisplayValueFalse);
			Assert.AreEqual(_DISPLAY_VALUE_TRUE, field.DisplayValueTrue);
			Assert.AreEqual(_ENABLE_DATA_GRID, field.EnableDataGrid);
			Assert.AreEqual(_FIELD_ARTIFACT_TYPE_ID, field.FieldArtifactTypeID);
			Assert.AreEqual(_FIELD_CATEGORY_CONVERTED, field.FieldCategory);
			Assert.AreEqual(_FIELD_DISPLAY_TYPE_ID, field.FieldDisplayTypeID);
			Assert.AreEqual(_fieldTreeView, field.FieldTreeView);
			Assert.AreEqual(_FIELD_TYPE_CONVERTED, field.FieldType);
			Assert.AreEqual(_FILTER_TYPE, field.FilterType);
			Assert.AreEqual(_FORMAT_STRING, field.FormatString);
			Assert.AreEqual(_FRIENDLY_NAME, field.FriendlyName);
			Assert.AreEqual(_guids, field.Guids);
			Assert.AreEqual(_IMPORT_BEHAVIOR_CONVERTED, field.ImportBehavior);
			Assert.AreEqual(_IS_ARTIFACT_BASE_FIELD, field.IsArtifactBaseField);
			Assert.AreEqual(_IS_AVAILABLE_IN_CHOICE_TREE, field.IsAvailableInChoiceTree);
			Assert.AreEqual(_IS_AVAILABLE_TO_ASSOCIATIVE_OBJECTS, field.IsAvailableToAssociativeObjects);
			Assert.AreEqual(_IS_EDITABLE, field.IsEditable);
			Assert.AreEqual(_IS_GROUP_BY_ENABLED, field.IsGroupByEnabled);
			Assert.AreEqual(_IS_INDEX_ENABLED, field.IsIndexEnabled);
			Assert.AreEqual(_IS_LINKED, field.IsLinked);
			Assert.AreEqual(_IS_REMOVABLE, field.IsRemovable);
			Assert.AreEqual(_IS_REQUIRED, field.IsRequired);
			Assert.AreEqual(_IS_SORTABLE, field.IsSortable);
			Assert.AreEqual(_IS_VISIBLE, field.IsVisible);
			Assert.IsTrue(AreKeyboardShortcutsEqual(_keyboardShortcutConverted, field.KeyboardShortcut));
			Assert.AreEqual(_KEYWORDS, field.Keywords);
			Assert.AreEqual(_LAST_MODIFIED_BY, field.LastModifiedBy);
			Assert.AreEqual(_lastModifiedOn, field.LastModifiedOn);
			Assert.AreEqual(_LINK_LAYOUT_ARTIFACT_ID, field.LinkLayoutArtifactID);
			Assert.AreEqual(_MAX_LENGTH, field.MaxLength);
			Assert.AreEqual(_NAME, field.NameValue);
			Assert.AreEqual(_NOTES, field.Notes);
			Assert.AreEqual(_overlayBehavior, field.OverlayBehavior);
			Assert.AreEqual(_parentArtifactID, field.ParentArtifactID);
			Assert.AreEqual(_popupPickerView, field.PopupPickerView);
			Assert.AreEqual(_relationalIndexViewArtifactID, field.RelationalIndexViewArtifactID);
			Assert.AreEqual(_TEXT_IDENTIFIER, field.TextIdentifier);
			Assert.AreEqual(_USE_UNICODE_ENCODING, field.UseUnicodeEncoding);
			Assert.AreEqual(_VALUE, field.Value);
			Assert.AreEqual(_WIDTH, field.Width);
			Assert.AreEqual(_WRAPPING, field.Wrapping);
		}

		private static bool AreKeyboardShortcutsEqual(KeyboardShortcut keyboardShortcut1, KeyboardShortcut keyboardShortcut2)
		{
			return keyboardShortcut1.Alt == keyboardShortcut2.Alt &&
			       keyboardShortcut1.Ctrl == keyboardShortcut2.Ctrl &&
			       keyboardShortcut1.Shift == keyboardShortcut2.Shift &&
			       keyboardShortcut1.Id == keyboardShortcut2.Id &&
			       keyboardShortcut1.Key == keyboardShortcut2.Key;
		}
	}
}