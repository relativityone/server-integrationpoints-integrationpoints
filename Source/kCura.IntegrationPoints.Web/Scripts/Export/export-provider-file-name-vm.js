﻿FileNameEntry = function (name, value, type) {
	this.name = name;
	this.value = value;
	this.type = type;
}

ExportProviderFileNameViewModel = function (availableFields, okCallback) {

	var self = this;

	var exportHelper = new ExportHelper();
	self.Max_Selection_Count = 5;

	self.availableFields = availableFields;
	self.actualSelectionTypeIndex = 0;
	self.okCallback = okCallback;

	self.metaData = ko.observableArray([]);
	self.listData = ko.observable({});
	self.data = ko.observable({});

	self.visibilityValuesContainer = ko.observableArray([]);

	self.applyCustomStyles = function() {
		for (var actualIndex = 0; actualIndex < self.Max_Selection_Count; ++actualIndex) {
			$($('#fileNamingContainer div.select2-container')[actualIndex])
				.addClass(actualIndex % 2 === 0 ? 'fileNamingType_field' : 'fileNamingType_separator');
		}
	}

	self.addNewSelection = function() {

		var actualIndex = self.metaData().length;

		self.metaData.push(actualIndex);
		self.visibilityValuesContainer()[actualIndex](true);
		self.visibilityValuesContainer()[actualIndex].notifySubscribers();
	};

	self.selectItem = function (fileNameEntry) {
		var index = self.metaData().length - 1;

		var selValue = fileNameEntry.value;
		// Value of dropdown Separator is display text as there is the issue with binding empty string (space char) for knockout Select2 ctrl
		if (fileNameEntry.type === 'S') {
			selValue = exportHelper.convertSeparatorValueToDisplay(fileNameEntry.value);
		}
		self.data()[index](selValue);
	}

	self.removeNewSelection = function() {
		self.metaData.pop();

		var actualIndex = self.metaData().length;
		self.visibilityValuesContainer()[actualIndex](false);
	};

	self.clearViewModel = function () {
		var selectionCount = self.metaData().length;
		for (var index = 0; index < selectionCount; ++index) {
			self.removeNewSelection();
		}
	}

	self.initViewModel = function () {

		ko.validation.rules['shouldBeValidated'] = {
			validator: function (val, currElementIndex) {
				if (currElementIndex >= self.metaData().length) {
					return true;
				}
				return val !== undefined && val != null;
			},
			message: "Please select value"
		}

		ko.validation.registerExtenders();

		for (var actualIndex = 0; actualIndex < self.Max_Selection_Count; ++actualIndex) {

			self.data()[actualIndex] = ko.observable().extend({
				shouldBeValidated: actualIndex
			});

			var newElem = ko.observable(false);
			self.visibilityValuesContainer.push(newElem);

			self.listData()[actualIndex] = ko.observableArray([]);

			if (actualIndex % 2 === 0) {
				for (var fieldIndex = 0; fieldIndex < self.availableFields.length; ++fieldIndex) {
					var field = self.availableFields[fieldIndex];
					self.listData()[actualIndex].push(new FileNameEntry(field.displayName, field.fieldIdentifier, "F"));
				}
			} else {
				for (var sepIndex = 0; sepIndex < ExportEnums.AvailableSeparators.length; ++sepIndex) {
					self.listData()[actualIndex].push(new FileNameEntry(ExportEnums.AvailableSeparators[sepIndex].display,
						ExportEnums.AvailableSeparators[sepIndex].value, "S"));
				}
			}
		}
	}
	self.getSelections = function () {
		var selections = [];
		for (var index = 0; index < self.metaData().length; ++index) {
			if (self.data()[index] !== undefined) {

				var fieldValue = self.data()[index]();

				//field
				if (index % 2 === 0) {
					var field = self.availableFields.find(function (element) {
						return element.fieldIdentifier === fieldValue;
					});

					selections.push(new FileNameEntry(field.displayName, fieldValue, "F"));
				}
				//separator
				else {
					selections.push(new FileNameEntry(fieldValue, exportHelper.convertSeparatorDisplayToValue(fieldValue), "S"));
				}
			}
		}
		return selections;
	}

	self.addButtonVisible = function () {
		return self.metaData().length < self.Max_Selection_Count;
	}

	self.delButtonVisible = function () {
		return self.metaData().length > 1;
	}

	this.construct = function (view) {
		self.view = view;
	};

	this.open = function (selectionList) {

		self.clearViewModel();
		self.selectionList = selectionList;

		if (self.selectionList !== undefined && self.selectionList.length > 0) {
			for (var selectionIndex = 0; selectionIndex < self.selectionList.length; ++selectionIndex) {
				self.addNewSelection();
				self.selectItem(self.selectionList[selectionIndex]);
			}
		} else {
			self.addNewSelection();
		}

		self.applyCustomStyles();
		self.view.dialog("open");
	};

	this.ok = function () {
		var modalErrors = ko.validation.group(self, { deep: true });
		if (modalErrors().length > 0) {
			modalErrors.showAllMessages(true);
			return false;
		}
		self.okCallback(self.getSelections());

		self.view.dialog("close");
	};

	this.cancel = function () {
		self.view.dialog("close");
	};

	self.initViewModel();
}