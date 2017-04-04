ListEntry = function (name, value, type) {
	this.name = name;
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

	self.addNewSelection = function(listEntrySelection) {

		var index = self.actualSelectionTypeIndex++;
		
		if (!ko.isObservable(self.listData()[index])) {
			self.listData()[index] = ko.observableArray([]);
		}
		
		if (!ko.isObservable(self.data()[index])){
		    self.data()[index] = ko.observable(); 
		}
		
		if (index % 2 === 0) {
			for (var fieldIndex = 0; fieldIndex < self.availableFields.length; ++fieldIndex) {
				var field = self.availableFields[fieldIndex];
				self.listData()[index].push(new ListEntry(field.displayName, field.fieldIdentifier));
				if (listEntrySelection !== undefined && listEntrySelection.value === field.fieldIdentifier) {
					self.data()[index](field.fieldIdentifier);
				}
			}
	

		} else {
			for (var sepIndex = 0; sepIndex < self.availableSeparators.length; ++sepIndex) {
				self.listData()[index].push(new ListEntry(self.availableSeparators[sepIndex], self.availableSeparators[sepIndex]));

				if (listEntrySelection !== undefined && listEntrySelection.value === self.availableSeparators[sepIndex]) {
					self.data()[index](self.availableSeparators[sepIndex]);
				}
			}
		}

		self.metaData.push(index);
	};
	
	self.removeNewSelection = function() {

		--self.actualSelectionTypeIndex;
		
		self.metaData.pop();
	};

	self.initViewModel = function (selectionList) {

		for (var selectionIndex = 0; selectionIndex < selectionList.length; ++selectionIndex) {

			self.addNewSelection(selectionList[selectionIndex]);
		}
	}


	self.initViewModel(selectionList);
}