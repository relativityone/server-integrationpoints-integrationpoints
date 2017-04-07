ListEntry = function (name, value, type) {
	this.name = name;
	this.value = value;
	this.type = type;
}

FileNameEntry = function (value, type) {
	this.value = value;
	this.type = type;
}

AvailableFieldMock = function (name, value) {
	this.displayName = name;
	this.fieldIdentifier = value;
}

ExportProviderFileNameViewModel = function (availableFields, selectionList) {

	var self = this;

	self.availableFields = availableFields;
	self.availableSeparators = ["-", "+", " "];

	self.selectionList = selectionList;

	self.actualSelectionTypeIndex = 0;

	self.metaData = ko.observableArray([]);
	self.listData = ko.observable({});
	self.data = ko.observable({});

	self.IsRequired = ko.observable(false);

	self.addNewSelection = function (fileNameEntry) {

		var index = self.actualSelectionTypeIndex++;

		if (!ko.isObservable(self.listData()[index])) {
			self.listData()[index] = ko.observableArray([]);
		}


		if (!ko.isObservable(self.data()[index])) {
			self.data()[index] = ko.observable().extend({
				required: { onlyIf: self.IsRequired }
			});
		}

		if (index % 2 === 0) {
			for (var fieldIndex = 0; fieldIndex < self.availableFields.length; ++fieldIndex) {
				var field = self.availableFields[fieldIndex];
				self.listData()[index].push(new ListEntry(field.displayName, field.fieldIdentifier));
				if (fileNameEntry !== undefined && fileNameEntry.value === field.fieldIdentifier) {
					self.data()[index](field.fieldIdentifier);
				}
			}
		} else {
			for (var sepIndex = 0; sepIndex < self.availableSeparators.length; ++sepIndex) {
				self.listData()[index].push(new ListEntry(self.availableSeparators[sepIndex], self.availableSeparators[sepIndex]));

				if (fileNameEntry !== undefined && fileNameEntry.value === self.availableSeparators[sepIndex]) {
					self.data()[index](self.availableSeparators[sepIndex]);
				}
			}
		}

		self.IsRequired(true);

		self.metaData.push(index);

		$($('#fileNamingContainer div.select2-container')[index]).addClass(index % 2 === 0 ? 'fileNamingType_field' : 'fileNamingType_separator')
	};

	self.removeNewSelection = function () {

		--self.actualSelectionTypeIndex;

		delete self.listData()[self.actualSelectionTypeIndex];
		delete self.data()[self.actualSelectionTypeIndex];
		$($('#fileNamingContainer div.select2-container')[self.actualSelectionTypeIndex]).removeClass('fileNamingType_field', 'fileNamingType_separator')
		self.metaData.pop();
	};

	self.initViewModel = function () {

		if (self.selectionList !== undefined) {
			for (var selectionIndex = 0; selectionIndex < self.selectionList.length; ++selectionIndex) {
				self.addNewSelection(self.selectionList[selectionIndex]);
			}
		} else {
			self.addNewSelection({});
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

	self.addButtonVisible = function () {
		return self.metaData().length < 5;
	}

	self.delButtonVisible = function () {
		return self.metaData().length > 1;
	}
}