var ExportHelper = function () {
	var self = this;

	self.convertFileNamePartsToText = function (fileNameParts) {
		var result = "";

		fileNameParts.forEach(function (part) {
			if (part.type === 'F') {
				result += "{" + part.name + "}";
			}
			else if (part.type === 'S') {
				result += part.value;
			}
		});

		return result;
	}

	self.convertSeparatorDisplayToValue = function(display) {
		
		if (display === ExportEnums.SeparatorsDefs.SpaceText) {
			return ExportEnums.SeparatorsDefs.SpaceVal;
		}
		else if (display === ExportEnums.SeparatorsDefs.NoneText) {
			return ExportEnums.SeparatorsDefs.NoneVal;
		}
		return display;
	}

	self.convertSeparatorValueToDisplay = function (value) {
		if (value === ExportEnums.SeparatorsDefs.SpaceVal) {
			return ExportEnums.SeparatorsDefs.SpaceText;
		}
		else if (value === ExportEnums.SeparatorsDefs.NoneVal) {
			return ExportEnums.SeparatorsDefs.NoneText;
		}
		return value;
	}
};