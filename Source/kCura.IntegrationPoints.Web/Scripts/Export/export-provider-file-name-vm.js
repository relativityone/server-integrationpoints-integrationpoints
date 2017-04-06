﻿ListEntry = function (name, value, type) {
	this.name = name;
	this.value = value;
	this.type = type;
}

FileNameEntry = function (value, type) {
	this.value = value;
	this.type = type;
}

AvailableFieldMock = function(name, value) {
	this.displayName = name;
	this.fieldIdentifier = value;
}

ExportProviderFileNameViewModel = function(availableFields, selectionList) {

	var self = this;

	self.Max_Selection_Count = 5;

	self.availableFields = availableFields;

	self.actualSelectionTypeIndex = 0;

	self.metaData = ko.observableArray([]);
	self.listData = ko.observable({});
	self.data = ko.observable({});

	self.addNewSelection = function() {

		var actualIndex = self.metaData().length;
		
		if (!ko.isObservable(self.listData()[actualIndex])) {
			self.listData()[actualIndex] = ko.observableArray([]);
		}
		
		if (actualIndex % 2 === 0) {
			for (var fieldIndex = 0; fieldIndex < self.availableFields.length; ++fieldIndex) {
				var field = self.availableFields[fieldIndex];
				self.listData()[actualIndex].push(new ListEntry(field.displayName, field.fieldIdentifier, "F"));
			}
		} else {
			for (var sepIndex = 0; sepIndex < ExportEnums.AvailableSeparators.length; ++sepIndex) {
				self.listData()[actualIndex].push(new ListEntry(ExportEnums.AvailableSeparators[sepIndex].display, ExportEnums.AvailableSeparators[sepIndex].value, "S"));
			}
		}

		self.metaData.push(actualIndex);
		self.data()[actualIndex](null);
	};

	self.selectItem = function (fileNameEntry) {
		var index = self.metaData().length - 1;
		self.data()[index](fileNameEntry.value);
	}

	self.removeNewSelection = function() {

		self.metaData.pop();

		delete self.listData()[self.metaData().length];
		delete self.data()[self.metaData().length];
	};

	self.initViewModel = function (selectionList) {

		for (var selIndex = 0; selIndex < self.Max_Selection_Count; ++selIndex) {

			ko.validation.rules['shouldBeValidated'] = {
				validator: function(val, currElementIndex) {
					if (currElementIndex >= self.metaData().length || self.metaData().length <= 1) {
						return true;
					}
					return val !== undefined && val != null;
				},
				message: "Please select value"
			}
	
			ko.validation.registerExtenders();

			self.data()[selIndex] = ko.observable().extend({
				shouldBeValidated: selIndex
			});
		}
		
		if (selectionList !== undefined) {
			for (var selectionIndex = 0; selectionIndex < selectionList.length; ++selectionIndex) {
				self.addNewSelection();
				self.selectItem(selectionList[selectionIndex]);
			}
		} else {
			self.addNewSelection();
		}
	}

	self.getSelections = function () {
		var selections = [];
		for (var index = 0; index < self.metaData().length; ++index) {
			if (self.data()[index] !== undefined) {
				selections.push(new FileNameEntry(self.data()[index](), index % 2 === 0 ? "F" : "S"));
			}
		}
		return selections;
	}

	self.addButtonVisible = function() {
		return self.metaData().length < self.Max_Selection_Count;
	}

	self.delButtonVisible = function () {
		return self.metaData().length > 1;
	}

	self.initViewModel(selectionList);
}