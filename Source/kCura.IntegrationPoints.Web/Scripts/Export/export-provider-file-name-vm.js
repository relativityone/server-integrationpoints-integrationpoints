FileNameEntry = function (name, value, type) {
	this.name = name;
	this.value = value;
	this.type = type;
}

ExportProviderFileNameViewModel = function(availableFields, selectionList) {

	var self = this;

	self.Max_Selection_Count = 5;

	self.availableFields = availableFields;

	self.actualSelectionTypeIndex = 0;

	self.metaData = ko.observableArray([]);
	self.listData = ko.observable({});
	self.data = ko.observable({});

	self.fieldFileNameEntries = [];
	self.sepFileNameEntries = [];

	self.addNewSelection = function() {

		var actualIndex = self.metaData().length;
		
		if (!ko.isObservable(self.listData()[actualIndex])) {
			self.listData()[actualIndex] = ko.observableArray([]);
		}

		//if (!ko.isObservable(self.data()[actualIndex])) {
		//	self.data()[actualIndex] = ko.observable().extend({
		//		shouldBeValidated: actualIndex
		//	});

		//	ko.validation.registerExtenders();
		//}
		
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

		self.metaData.push(actualIndex);
	};

	self.selectItem = function (fileNameEntry) {
		var index = self.metaData().length - 1;
		self.data()[index](fileNameEntry.value);
	}

	self.removeNewSelection = function() {

		self.metaData.pop();

		//var delSelIndex = self.metaData().length;

		//self.data()[delSelIndex].rules({ validatable: false });

		//delete self.listData()[delSelIndex];
		//delete self.data()[delSelIndex];

		//if (!ko.isObservable(self.data()[delSelIndex])) {
		//	self.data()[delSelIndex] = ko.observable().extend({
		//		shouldBeValidated: delSelIndex
		//	});
		//}
	};

	self.initViewModel = function (selectionList) {

		ko.validation.rules['shouldBeValidated'] = {
			validator: function (val, currElementIndex) {
				if (currElementIndex >= self.metaData().length || self.metaData().length <= 1) {
					return true;
				}
				return val !== undefined && val != null;
			},
			message: "Please select value {0}"
		}

		ko.validation.registerExtenders();

		for (var selIndex = 0; selIndex < self.Max_Selection_Count; ++selIndex) {

			self.data()[selIndex] = ko.observable().extend({
				shouldBeValidated: selIndex
			});

			//self.data()[selIndex] = ko.observable().extend({
			//	validation: function (val, currElementIndex) {
			//		if (currElementIndex >= self.metaData().length || self.metaData().length <= 1) {
			//			return true;
			//		}
			//		return val !== undefined && val != null;
			//	},
			//	message: "Please select value {0}"
			//});
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