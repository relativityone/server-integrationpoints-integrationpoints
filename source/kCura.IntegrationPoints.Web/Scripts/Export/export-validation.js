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
		return ko.validation.rules['pattern'].validator(fieldValue, { regExp: ValidationPatterns.textFieldWithoutForbiddenCharacters });
	},
	message: 'Field cannot contain special characters such as: < > : " \\ / | ? *'
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