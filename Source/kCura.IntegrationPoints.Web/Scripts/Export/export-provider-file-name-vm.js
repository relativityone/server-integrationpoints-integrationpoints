FileNameEntry = function (name, value, type) {
	this.name = name;
	this.value = value;
	this.type = type;
}

ExportProviderFileNameViewModel = function (availableFields, selectionList) {

	var self = this;

	self.Max_Selection_Count = 5;

	self.availableFields = availableFields;
	self.selectionList = selectionList;
	self.actualSelectionTypeIndex = 0;

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
		self.data()[index](fileNameEntry.value);
	}

	self.removeNewSelection = function() {
		self.metaData.pop();

		var actualIndex = self.metaData().length;
		self.visibilityValuesContainer()[actualIndex](false);
	};

	self.initViewModel = function () {

		ko.validation.rules['shouldBeValidated'] = {
			validator: function (val, currElementIndex) {
				if (currElementIndex >= self.metaData().length || self.metaData().length <= 1) {
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
					self.listData()[actualIndex].push(new FileNameEntry(ExportEnums.AvailableSeparators[sepIndex].display, ExportEnums.AvailableSeparators[sepIndex].value, "S"));
				}
			}
		}

		if (self.selectionList !== undefined) {
			for (var selectionIndex = 0; selectionIndex < self.selectionList.length; ++selectionIndex) {
				self.addNewSelection();
				self.selectItem(self.selectionList[selectionIndex]);
			}
		} else {
			self.addNewSelection();
		}
	}
	self.getSelections = function () {
		var selections = [];
		for (var index = 0; index < self.metaData().length; ++index) {
			if (self.data()[index] !== undefined) {
				selections.push(new FileNameEntry("", self.data()[index](), index % 2 === 0 ? "F" : "S"));
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
}