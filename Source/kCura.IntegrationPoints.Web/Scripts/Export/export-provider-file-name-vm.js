ListEntry = function (name, value, type) {
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

	self.availableFields = availableFields;
	self.availableSeparators = ["-", "+", " "];

	self.actualSelectionTypeIndex = 0;

	self.metaData = ko.observableArray([]);
	self.listData = ko.observable({});
	self.data = ko.observable({});

	self.IsRequired = ko.observable(false);

	self.addNewSelection = function(fileNameEntry) {

		var index = self.actualSelectionTypeIndex++;
		
		if (!ko.isObservable(self.listData()[index])) {
			self.listData()[index] = ko.observableArray([]);
		}
		
		
		if (!ko.isObservable(self.data()[index])){
			self.data()[index] = ko.observable().extend({
				required: {onlyIf: self.IsRequired}
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
	};
	
	self.removeNewSelection = function() {

		--self.actualSelectionTypeIndex;

		delete self.listData()[self.actualSelectionTypeIndex];
		delete self.data()[self.actualSelectionTypeIndex];

		self.metaData.pop();
	};

	self.initViewModel = function (selectionList) {

		if (selectionList !== undefined) {
			for (var selectionIndex = 0; selectionIndex < selectionList.length; ++selectionIndex) {
				self.addNewSelection(selectionList[selectionIndex]);
			}
		} else {
			self.addNewSelection({});
		}
	}

	self.initViewModel(selectionList);

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
		return self.metaData().length < 5;
	}

	self.delButtonVisible = function () {
		return self.metaData().length > 1;
	}
}