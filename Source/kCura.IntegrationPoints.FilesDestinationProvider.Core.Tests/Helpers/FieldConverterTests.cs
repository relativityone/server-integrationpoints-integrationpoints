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
    [TestFixture, Category("Unit")]
    public class FieldConverterTests
    {
        [Test]
        public void ConvertToFieldTest()
        {
            // Arrange
            IField field = TestFields.CreateBasicFieldMock().Object;
            var converter = new FieldConverter();

            // Act
            Field result = converter.ConvertToField(field);

            // Assert
            TestFields.ValidateConvertedFieldValues(result, field);
        }

        [Test]
        public void ConvertToFieldWithEmptyAndNullValuesTest()
        {
            // Arrange
            IField field = TestFields.CreateFieldMockWithNulls().Object;
            var converter = new FieldConverter();

            // Act
            Field result = converter.ConvertToField(field);

            // Assert
            TestFields.ValidateConvertedFieldValues(result, field);
        }

        public static class TestFields
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
            private const bool _WRAPPING = true;
            private const FieldCategory _FIELD_CATEGORY = FieldCategory.Batch;
            private const FieldType _FIELD_TYPE = FieldType.Code;
            private const int _ARTIFACT_ID = 1017415;
            private const int _ARTIFACT_TYPE_ID = 10;
            private const int _CREATED_BY = 1049713;
            private const int _FIELD_ARTIFACT_TYPE_ID = 11;
            private const int _FIELD_DISPLAY_TYPE_ID = 12;
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

            private static readonly bool? _overlayBehavior = true;
            private static readonly DateTime _createdOn = new DateTime(2019, 2, 1);
            private static readonly DateTime _lastModifiedOn = new DateTime(2019, 2, 2);
            private static readonly FieldImportBehavior? _IMPORT_BEHAVIOR = FieldImportBehavior.ObjectFieldContainsArtifactId;
            private static readonly IList<Guid> _guids = new List<Guid>
            {
                Guid.Parse("4D8763F2-1EF2-478F-8AB7-0A25E902DACF"),
                Guid.Parse("448D35D6-552A-4C14-B59D-0F3ABCF1BFF2")
            };
            private static readonly int? _associativeArtifactTypeID = 1031234;
            private static readonly int? _codeTypeID = 11;
            private static readonly int? _fieldTreeView = 13;
            private static readonly int? _parentArtifactID = 1074747;
            private static readonly int? _popupPickerView = 16;
            private static readonly int? _relationalIndexViewArtifactID = 1084387;
            private static readonly Mock<IKeyboardShortcut> _keyboardShortcut = CreateKeyboardShortcutMock();

            private const bool _KEYBOARD_SHORTCUT_ALT = false;
            private const bool _KEYBOARD_SHORTCUT_CTRL = false;
            private const bool _KEYBOARD_SHORTCUT_SHIFT = true;
            private const bool _USE_UNICODE_ENCODING = true;

            internal static void ValidateConvertedFieldValues(Field field, IField sourceField)
            {
                Assert.AreEqual(sourceField.AllowGroupBy, field.AllowGroupBy);
                Assert.AreEqual(sourceField.AllowHtml, field.AllowHtml);
                Assert.AreEqual(sourceField.AllowPivot, field.AllowPivot);
                Assert.AreEqual(sourceField.ArtifactID, field.ArtifactID);
                Assert.AreEqual(sourceField.ArtifactTypeID, field.ArtifactTypeID);
                Assert.AreEqual(sourceField.ViewFieldID, field.ArtifactViewFieldID);
                Assert.AreEqual(sourceField.AssociativeArtifactTypeID, field.AssociativeArtifactTypeID);
                Assert.AreEqual(sourceField.AutoAddChoices, field.AutoAddChoices);
                Assert.AreEqual(sourceField.AvailableInViewer, field.AvailableInViewer);
                Assert.AreEqual(sourceField.CodeTypeID, field.CodeTypeID);
                Assert.AreEqual(sourceField.ColumnName, field.ColumnName);
                Assert.AreEqual(sourceField.CreatedBy, field.CreatedBy);
                Assert.AreEqual(sourceField.CreatedOn, field.CreatedOn);
                Assert.AreEqual(sourceField.DeleteFlag, field.DeleteFlag);
                Assert.AreEqual(sourceField.DisplayValueFalse, field.DisplayValueFalse);
                Assert.AreEqual(sourceField.DisplayValueTrue, field.DisplayValueTrue);
                Assert.AreEqual(sourceField.EnableDataGrid, field.EnableDataGrid);
                Assert.AreEqual(sourceField.FieldArtifactTypeID, field.FieldArtifactTypeID);
                AssertEqualEnums(sourceField.FieldCategory, field.FieldCategory);
                Assert.AreEqual(sourceField.FieldDisplayTypeID, field.FieldDisplayTypeID);
                Assert.AreEqual(sourceField.FieldTreeView, field.FieldTreeView);
                AssertEqualEnums(sourceField.FieldType, field.FieldType);
                Assert.AreEqual(sourceField.FilterType, field.FilterType);
                Assert.AreEqual(sourceField.FormatString, field.FormatString);
                Assert.AreEqual(sourceField.FriendlyName, field.FriendlyName);
                CollectionAssert.AreEqual(sourceField.Guids, field.Guids);
                AssertEqualEnums(sourceField.ImportBehavior, field.ImportBehavior);
                Assert.AreEqual(sourceField.IsArtifactBaseField, field.IsArtifactBaseField);
                Assert.AreEqual(sourceField.IsAvailableInChoiceTree, field.IsAvailableInChoiceTree);
                Assert.AreEqual(sourceField.IsAvailableToAssociativeObjects, field.IsAvailableToAssociativeObjects);
                Assert.AreEqual(sourceField.IsEditable, field.IsEditable);
                Assert.AreEqual(sourceField.IsGroupByEnabled, field.IsGroupByEnabled);
                Assert.AreEqual(sourceField.IsIndexEnabled, field.IsIndexEnabled);
                Assert.AreEqual(sourceField.IsLinked, field.IsLinked);
                Assert.AreEqual(sourceField.IsRemovable, field.IsRemovable);
                Assert.AreEqual(sourceField.IsRequired, field.IsRequired);
                Assert.AreEqual(field.IsSortable, field.IsSortable);
                Assert.AreEqual(sourceField.IsVisible, field.IsVisible);
                Assert.IsTrue(AreKeyboardShortcutsEqual(sourceField.KeyboardShortcut, field.KeyboardShortcut));
                Assert.AreEqual(sourceField.Keywords, field.Keywords);
                Assert.AreEqual(sourceField.LastModifiedBy, field.LastModifiedBy);
                Assert.AreEqual(sourceField.LastModifiedOn, field.LastModifiedOn);
                Assert.AreEqual(sourceField.LinkLayoutArtifactID, field.LinkLayoutArtifactID);
                Assert.AreEqual(sourceField.MaxLength, field.MaxLength);
                Assert.AreEqual(sourceField.Name, field.NameValue);
                Assert.AreEqual(sourceField.Name, field.DisplayName);
                Assert.AreEqual(sourceField.Notes, field.Notes);
                Assert.AreEqual(sourceField.OverlayBehavior, field.OverlayBehavior);
                Assert.AreEqual(sourceField.ParentArtifactID, field.ParentArtifactID);
                Assert.AreEqual(sourceField.PopupPickerView, field.PopupPickerView);
                Assert.AreEqual(sourceField.RelationalIndexViewArtifactID, field.RelationalIndexViewArtifactID);
                Assert.AreEqual(sourceField.TextIdentifier, field.TextIdentifier);
                Assert.AreEqual(_USE_UNICODE_ENCODING, field.UseUnicodeEncoding);
                Assert.AreEqual(sourceField.Value, field.Value);
                Assert.AreEqual(sourceField.Width, field.Width);
                Assert.AreEqual(sourceField.Wrapping, field.Wrapping);
            }

            private static void AssertEqualEnums(object enum1, object enum2)
            {
                if (enum1 != null && enum2 != null)
                {
                    Assert.AreEqual(enum1.ToString(), enum2.ToString());
                    Assert.AreEqual((int)enum1, (int)enum2);
                }
                else if ((enum1 == null && enum2 != null) || (enum2 == null && enum1 != null))
                {
                    Assert.Fail($"Enum values are not equal expected {enum1} actual {enum2}");
                }
            }

            internal static Mock<IField> CreateBasicFieldMock()
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

            private static bool AreKeyboardShortcutsEqual(IKeyboardShortcut keyboardShortcut1, KeyboardShortcut keyboardShortcut2)
            {
                if ((keyboardShortcut1 != null || keyboardShortcut2 == null) &&
                    (keyboardShortcut2 != null || keyboardShortcut1 == null))
                {
                    if (keyboardShortcut1 != null && keyboardShortcut2 != null)
                    {
                        return keyboardShortcut1.Alt == keyboardShortcut2.Alt &&
                               keyboardShortcut1.Ctrl == keyboardShortcut2.Ctrl &&
                               keyboardShortcut1.Shift == keyboardShortcut2.Shift &&
                               keyboardShortcut1.ID == keyboardShortcut2.Id &&
                               keyboardShortcut1.Key == keyboardShortcut2.Key;
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;
            }

            private const int _KEYBOARD_SHORTCUT_ID = 1048234;

            private const int _KEYBOARD_SHORTCUT_KEY = 14;

            internal static Mock<IField> CreateFieldMockWithNulls()
            {
                Mock<IField> field = CreateBasicFieldMock();
                field.Setup(x => x.ImportBehavior).Returns<ImportBehaviorChoice?>(null);
                field.Setup(x => x.Guids).Returns<IList<Guid>>(null);
                field.Setup(x => x.KeyboardShortcut).Returns<IKeyboardShortcut>(null);
                return field;
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

        }
    }
}
