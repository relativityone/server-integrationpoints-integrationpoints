ListEntry = function (name, value) {
	this.name = name;
	this.value = value;
}

ExportProviderFileNameViewModel = function(availableFields) {

	var self = this;

	self.availableFields = availableFields;
	self.availableSeparators = ['Control Number', 'Case Admin - Production Volume', 'Extracted Text'];

	self.actualSelectionTypeIndex = 0;

	self.metaData = ko.observableArray([]);
	self.listData = ko.observable({});
	self.data = ko.observable({});

	self.addNewSelection = function() {

		var index = self.actualSelectionTypeIndex++;
		
		if (!ko.isObservable(self.listData()[index])) {
			self.listData()[index] = ko.observableArray([]);
			
			for (var i = 0; i < self.availableSeparators.length; ++i) {
				self.listData()[index].push(new ListEntry(self.availableSeparators[i], i));
			}
		}
		
		if (!ko.isObservable(self.data()[index])){
            self.data()[index] = ko.observable('Extracted Text'); 
		}
		
		// if (index % 2 === 0) {
			// for (var i = 0; i < self.availableFields.length; ++i) {
				// var field = self.availableFields[i];
				// self.listData()[index].push(new ListEntry(field.displayName, field.fieldIdentifier));
			// }

		// } else {
			// for (var i = 0; i < self.availableSeparators.length; ++i) {
				// self.listData()[index].push(new ListEntry(self.availableSeparators[i], i));
			// }
		// }

		self.metaData.push(index);
	};
	
	self.removeNewSelection = function() {

		var index = --self.actualSelectionTypeIndex;
		
		self.metaData.pop();
	};

	self.addNewSelection();
}