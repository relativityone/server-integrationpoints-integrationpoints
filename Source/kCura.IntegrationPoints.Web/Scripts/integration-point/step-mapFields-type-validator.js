var StepMapFieldsTypeValidator = (function() {

	var validateMappedFieldTypes = function(mapping) {
		var sourceMappedFields = mapping.sourceMapped;
		var destinationMappedFields = mapping.mappedWorkspace;
		var mismatchedMappings = [];

		var length = sourceMappedFields.length;
		for (var i = 0; i < length; i++) {
			var sourceField = sourceMappedFields[i];
			var destinationField = destinationMappedFields[i];
			if (sourceField.type !== destinationField.type) {
				var mappingEntry = {
					source: sourceField.displayName,
					destination: destinationField.displayName
				}
				mismatchedMappings.push(mappingEntry);
			}
		}

		return mismatchedMappings;
	};

	var buildMismatchedFieldTypesMessage = function(mismatchedMappings) {
		var message = "Data type mismatch for the following mapped field(s):<br />";
		for (var i = 0; i < mismatchedMappings.length; ++i) {
			var messagePart = " - " + mismatchedMappings[i].source + " : " + mismatchedMappings[i].destination + "<br />";
			message += messagePart;
		}
		var messageSuffix = "Continue with this mapping?";
		message += messageSuffix;
		return message;
	}

	var showWarningPopup = function(mismatchedMappings, successCallback) {
		var mismatchedMappingsMessage = buildMismatchedFieldTypesMessage(mismatchedMappings);
		window.Dragon.dialogs.showConfirm({
			message: mismatchedMappingsMessage,
			title: "Integration Point Validation",
			width: 450,
			showCancel: true,
			messageAsHtml: true,
			success: function(calls) {
				calls.close();
				successCallback();
			}
		});
	}

	return {
		validateMappedFieldTypes: validateMappedFieldTypes,
		showWarningPopup: showWarningPopup
	};
})();
