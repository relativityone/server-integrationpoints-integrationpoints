
var StepMapFieldsValidator = (function () {
	var isFieldMapEqual = function isFieldMapEqual(sourceFieldMap, destinationFieldMap) {
		return sourceFieldMap.sourceField.displayName == destinationFieldMap.sourceField.displayName
			&& sourceFieldMap.destinationField.displayName == destinationFieldMap.destinationField.displayName;
	}

	function getMissmatchedFieldsAsString(fieldMap) {
		return fieldMap.sourceField.displayName + ' [' + fieldMap.sourceField.type + ']' + ' : ' + fieldMap.destinationField.displayName + ' [' + fieldMap.destinationField.type + ']';
	}

	var buildFieldsMapTableMessage = function (mappedFields) {
		var mappedFieldsList = $('<div/>');

		$('<ul/>').appendTo(mappedFieldsList);

		mappedFields.forEach(
			function (fieldMap) {
				$('<li/>').text(getMissmatchedFieldsAsString(fieldMap)).appendTo(mappedFieldsList);
			});

		return mappedFieldsList;
	}

	var showProceedConfirmationPopup = function (invalidMappedFields, proceedCallback, clearAndProceedCallback) {
		var content = $('<div/>')
			.html('<p>Your job may be unsuccessfully finished by those Source and Destination fields:</p>');

		buildFieldsMapTableMessage(invalidMappedFields)
			.appendTo(content);

		$('<div/>').html(
			'<p>' +
			'Proceed: continue with current mapping <br/> ' +
			'Clear and Proceed: filter out invalid fields and continue <br/>' +
			'</p> ').appendTo(content);

		window.Dragon.dialogs.showYesNoCancel({
			title: "Field Map Validation",
			message: content.html(),
			messageAsHtml: true,
			yesText: "Proceed",
			yesHandle: function (calls) {
				calls.close();
				proceedCallback();
			},
			noText: "Clear and Proceed",
			noHandle: function (calls) {
				calls.close();
				clearAndProceedCallback()
			},
			closeOnEscape: false
		});
	}

	return {
		showProceedConfirmationPopup: showProceedConfirmationPopup,
		isFieldMapEqual: isFieldMapEqual
	};
})();
