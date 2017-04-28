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
};