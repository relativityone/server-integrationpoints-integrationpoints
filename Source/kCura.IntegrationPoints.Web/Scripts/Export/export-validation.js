ko.validation.rules['pattern'] = {
	validator: function (fieldValue, params) {
		if (params === null) {
			return false;
		}
		return params.regExp.test(fieldValue);
	}
};

ko.validation.rules['textFieldWithoutSpecialCharacters'] = {
	validator: function (fieldValue) {
		var areAllPrintableCharactersValid =
			ko.validation.rules['pattern'].validator(fieldValue,
				{ regExp: ValidationPatterns.textFieldWithoutForbiddenCharacters });
		var areAllCharacteresAbove31AsciiCode = fieldValue.split('')
			.map(function (x) {
				return x.charCodeAt(0);
			})
			.map(function (x) {
				var lastNonPrintableAsciiCharacter = 31;
				return x > lastNonPrintableAsciiCharacter;
			})
			.reduce(function (previous, current) {
				return previous && current;
			}, true);
		return areAllPrintableCharactersValid && areAllCharacteresAbove31AsciiCode;
	},
	message: 'Field cannot contain special characters such as: < > : " \\ / | ? * TAB'
};

ko.validation.rules['nonNegativeNaturalNumber'] = {
	validator: function (fieldValue) {
		return ko.validation.rules['pattern'].validator(fieldValue, { regExp: ValidationPatterns.nonNegativeNaturalNumber });
	},
	message: 'Field value has to be natural positive number'
};

ko.validation.init({
	registerExtenders: true,
	messagesOnModified: true,
	insertMessages: true,
	parseInputAttributes: true,
	messageTemplate: null
}, true);

var ValidationPatterns = {
	nonNegativeNaturalNumber: /^(\d)+$/,
	textFieldWithoutForbiddenCharacters: /^[^<>:\"\\\/|\?\*]*$/
};