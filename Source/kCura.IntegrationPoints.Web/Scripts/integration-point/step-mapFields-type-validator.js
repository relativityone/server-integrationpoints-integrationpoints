﻿
var StepMapFieldsValidator = (function () {
	var isFieldMapEqual = function isFieldMapEqual(sourceFieldMap, destinationFieldMap) {
		return sourceFieldMap.sourceField.displayName == destinationFieldMap.sourceField.displayName
			&& sourceFieldMap.destinationField.displayName == destinationFieldMap.destinationField.displayName;
	}

	function getMismatchedFieldsAsString(fieldMap) {
		return fieldMap.sourceField.displayName + ' [' + fieldMap.sourceField.type + ']' + ' : ' + fieldMap.destinationField.displayName + ' [' + fieldMap.destinationField.type + ']';
	}

	var buildFieldsMapTableMessage = function (mappedFields) {
		var mappedFieldsList = $('<div/>')
			.html('<p id="mappedFieldsWarning">Mapping of the fields below may fail your job:</p>');
		
		mappedFields.forEach(
			function (invalidMap, i) {
				$('<li/>', { id: 'invalidMap-' + i }).text(getMismatchedFieldsAsString(invalidMap.fieldMap)).appendTo(mappedFieldsList);
				reasonList = $('<ul/>')
				reasonList.appendTo(mappedFieldsList);
				invalidMap.invalidReasons.forEach(
					function (reason, j) {
						$('<li/>', { id: 'invalidReasons-' + i + '-' + j }).text(reason).appendTo(reasonList);
					});
			});

		return mappedFieldsList;
	}

	var showProceedConfirmationPopup = function (invalidMappedFields, isObjectIdentifierMapValid, proceedCallback, clearAndProceedCallback) {
				
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

		var content = $('<div/>');
		var proceedLabel = '';
		var clearAndProceedLabel = '';

		if (!isObjectIdentifierMapValid) {
			$('<p id="objectIdentifierWarning"/>').html(
				'The Source Maximum Length of the Object Identifier is greater than the one in Destination.<br/>' +
				'If you want to adjust it click Cancel, if not click Proceed to continue with current mapping.'
			).appendTo(content);
		}
		
		if (invalidMappedFields.length > 0) {
			buildFieldsMapTableMessage(invalidMappedFields).appendTo(content);
			proceedLabel = '<b>Proceed</b>: continue with current mapping <br/> ';
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
			proceedLabel +
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
