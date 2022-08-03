using kCura.EDDS.WebAPI.FieldManagerBase;
using Relativity.API.Foundation;
using System;
using System.Linq;
using FieldCategory = kCura.EDDS.WebAPI.FieldManagerBase.FieldCategory;
using FieldType = kCura.EDDS.WebAPI.FieldManagerBase.FieldType;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    internal class FieldConverter
    {
        public Field ConvertToField(IField field)
        {
            if (field == null)
            {
                return null;
            }

            return new Field
            {
                AllowGroupBy = field.AllowGroupBy,
                AllowHtml = field.AllowHtml,
                AllowPivot = field.AllowPivot,
                ArtifactID = field.ArtifactID,
                ArtifactTypeID = field.ArtifactTypeID,
                ArtifactViewFieldID = field.ViewFieldID,
                AssociativeArtifactTypeID = field.AssociativeArtifactTypeID,
                AutoAddChoices = field.AutoAddChoices,
                AvailableInViewer = field.AvailableInViewer,
                CodeTypeID = field.CodeTypeID,
                ColumnName = field.ColumnName,
                CreatedBy = field.CreatedBy,
                CreatedOn = field.CreatedOn,
                DeleteFlag = field.DeleteFlag,
                DisplayName = field.Name,
                DisplayValueFalse = field.DisplayValueFalse,
                DisplayValueTrue = field.DisplayValueTrue,
                EnableDataGrid = field.EnableDataGrid,
                FieldArtifactTypeID = field.FieldArtifactTypeID,
                FieldCategory = ConvertEnum<FieldCategory>(field.FieldCategory),
                FieldDisplayTypeID = field.FieldDisplayTypeID,
                FieldTreeView = field.FieldTreeView,
                FieldType = ConvertEnum<FieldType>(field.FieldType),
                FilterType = field.FilterType,
                FormatString = field.FormatString,
                FriendlyName = field.FriendlyName,
                Guids = field.Guids?.ToArray(),
                ImportBehavior = ConvertEnum<ImportBehaviorChoice?>(field.ImportBehavior),
                IsArtifactBaseField = field.IsArtifactBaseField,
                IsAvailableInChoiceTree = field.IsAvailableInChoiceTree,
                IsAvailableToAssociativeObjects = field.IsAvailableToAssociativeObjects,
                IsEditable = field.IsEditable,
                IsGroupByEnabled = field.IsGroupByEnabled,
                IsIndexEnabled = field.IsIndexEnabled,
                IsLinked = field.IsLinked,
                IsRemovable = field.IsRemovable,
                IsRequired = field.IsRequired,
                IsSortable = field.IsSortable,
                IsVisible = field.IsVisible,
                KeyboardShortcut = ConvertFieldShortcut(field.KeyboardShortcut),
                Keywords = field.Keywords,
                LastModifiedBy = field.LastModifiedBy,
                LastModifiedOn = field.LastModifiedOn,
                LinkLayoutArtifactID = field.LinkLayoutArtifactID,
                MaxLength = field.MaxLength,
                NameValue = field.Name,
                Notes = field.Notes,
                OverlayBehavior = field.OverlayBehavior,
                ParentArtifactID = field.ParentArtifactID,
                PopupPickerView = field.PopupPickerView,
                RelationalIndexViewArtifactID = field.RelationalIndexViewArtifactID,
                TextIdentifier = field.TextIdentifier,
                UseUnicodeEncoding = field.UseUnicodeEncoding,
                Value = field.Value,
                Width = field.Width,
                Wrapping = field.Wrapping
            };
        }

        private KeyboardShortcut ConvertFieldShortcut(IKeyboardShortcut fieldKeyboardShortcut)
        {
            if (fieldKeyboardShortcut == null)
            {
                return null;
            }
            return new KeyboardShortcut
            {
                Alt = fieldKeyboardShortcut.Alt,
                Ctrl = fieldKeyboardShortcut.Ctrl,
                Id = fieldKeyboardShortcut.ID,
                Key = fieldKeyboardShortcut.Key,
                Shift = fieldKeyboardShortcut.Shift
            };
        }

        private TEnum ConvertEnum<TEnum>(Enum source)
        {
            if (source == null)
            {
                return default(TEnum);
            }

            Type type = typeof(TEnum);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments().First();
            }

            return (TEnum)Enum.Parse(type, source.ToString(), true);
        }

    }
}
