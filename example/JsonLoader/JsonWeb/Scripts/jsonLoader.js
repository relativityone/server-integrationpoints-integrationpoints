﻿
var IP  = IP || {};

(function () {
	
	var message = IP.frameMessaging();
	var pageModel = {};
	var self = this;
	message.subscribe('submit', function () {
		
		var localModel = ko.toJS(pageModel);
		this.publish("saveState", localModel); //save the model incase of error
		this.publish('saveComplete', localModel);

});
	var viewModel = function (model) {
		var self = this;
		this.fieldLocation = ko.observable(model.fieldLocation);
		this.dataLocation = ko.observable(model.dataLocation);

	};

	message.subscribe("back", function () {
		this.publish("saveState", ko.toJS(pageModel));
	});

	message.subscribe('load', function (model) {
		if (typeof (model) === "string") {
			try {
				model = JSON.parse(model);
			} catch (e) {
				model = {};
			}
		}
		pageModel = new viewModel(model);
		ko.applyBindings(pageModel, document.getElementById('jsonConfiguration'));
	});

		
	
})();