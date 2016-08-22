var ListPickerViewModel = function() {
	var self = this;

	self.getMappedValues = function(values, comparer) {
		var _values = ko.utils.arrayMap(values,
			function(_item1) {
				var _value = ko.utils.arrayFilter(self.availableValues(),
					function(_item2) {
						return comparer(_item1, _item2);
					});
				return _value[0];
			});
		return _values;
	};

	self.selectValues = function(values, comparer) {
		self.mappedValues([]);

		var mappedValues = self.getMappedValues(values, comparer);

		self.selectedAvailableValues(mappedValues);
		self.addValue();
	};
	self.availableValues = ko.observableArray([]);
	self.selectedAvailableValues = ko.observableArray([]);
	self.mappedValues = ko.observableArray([]);

	self.selectedMappedValues = ko.observableArray([]);

	self.addValue = function() {
		IP.workspaceFieldsControls.add(
			self.availableValues,
			self.selectedAvailableValues,
			self.mappedValues
		);
	};

	self.addAllValues = function() {
		IP.workspaceFieldsControls.add(
			self.availableValues,
			self.availableValues,
			self.mappedValues
		);
	};

	self.removeValue = function() {
		IP.workspaceFieldsControls.add(
			self.mappedValues,
			self.selectedMappedValues,
			self.availableValues
		);
	};

	self.removeAllValues = function() {
		IP.workspaceFieldsControls.add(
			self.mappedValues,
			self.mappedValues,
			self.availableValues
		);
	};

	self.moveValueTop = function() {
		IP.workspaceFieldsControls.moveTop(
			self.mappedValues,
			self.selectedMappedValues()
		);
	};

	self.moveValueUp = function() {
		IP.workspaceFieldsControls.up(
			self.mappedValues,
			self.selectedMappedValues
		);
	};

	self.moveValueDown = function() {
		IP.workspaceFieldsControls.down(
			self.mappedValues,
			self.selectedMappedValues
		);
	};

	self.moveValueBottom = function() {
		IP.workspaceFieldsControls.moveBottom(
			self.mappedValues,
			self.selectedMappedValues()
		);
	};
};