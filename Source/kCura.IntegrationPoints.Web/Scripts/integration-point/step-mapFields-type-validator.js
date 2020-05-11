
var StepMapFieldsValidator = (function () {
	var isFieldMapEqual = function isFieldMapEqual(sourceFieldMap, destinationFieldMap) {
		return sourceFieldMap.sourceField.displayName == destinationFieldMap.sourceField.displayName
			&& sourceFieldMap.destinationField.displayName == destinationFieldMap.destinationField.displayName;
	}

	function getMismatchedFieldsAsString(fieldMap) {
		return fieldMap.sourceField.displayName + ' [' + fieldMap.sourceField.type + ']' + ' : ' + fieldMap.destinationField.displayName + ' [' + fieldMap.destinationField.type + ']';
	}

	var buildFieldsMapTableMessage = function (mappedFields, isObjectIdentifierMapValid) {
		var mappedFieldsList = $('<div/>');

		$('<ul/>').appendTo(mappedFieldsList);

		if (!isObjectIdentifierMapValid) {
			$('<li/>').text('The Source Maximum Length of the Object Identifier is greater than the one in Destination').appendTo(mappedFieldsList);
		}

		mappedFields.forEach(
			function (fieldMap) {
				$('<li/>').text(getMismatchedFieldsAsString(fieldMap)).appendTo(mappedFieldsList);
			});

		return mappedFieldsList;
	}

	var showProceedConfirmationPopup = function (invalidMappedFields, isObjectIdentifierMapValid, proceedCallback, clearAndProceedCallback) {
		var content = $('<div/>')
			.html('<p>Mapping of the fields below may fail your job:</p>');

		buildFieldsMapTableMessage(invalidMappedFields, isObjectIdentifierMapValid)
			.appendTo(content);

		var dialogOptions = {
			title: "Field Map Validation",
			messageAsHtml: true,
			yesText: "Proceed",
			yesHandle: function (calls) {
				calls.close();
				proceedCallback();
			},
			showNo: false,
			closeOnEscape: false
		};

		var clearAndProceedLabel = '';

		if (invalidMappedFields.length > 0) {

			clearAndProceedLabel = '<b>Clear and Proceed</b>: filter out the fields above (except Object Identifier) and continue <br/>';

			dialogOptions = Object.assign(dialogOptions, {
				noText: "Clear and Proceed",
				noHandle: function (calls) {
					calls.close();
					clearAndProceedCallback()
				},
				showNo: true
			});
		}

		$('<div/>').html(
			'<p>' +
			'<b>Proceed</b>: continue with current mapping <br/> ' +
			clearAndProceedLabel +
			'</p> ').appendTo(content);

		dialogOptions = Object.assign(dialogOptions, {
			message: content.html(),
		});

		window.Dragon.dialogs.showYesNoCancel(dialogOptions);
	}

	return {
		showProceedConfirmationPopup: showProceedConfirmationPopup,
		isFieldMapEqual: isFieldMapEqual
	};
})();
