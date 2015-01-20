
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
	var viewModel = function () {
		var self = this;
		this.fieldLocation = ko.observable("");
		this.dataLocation = ko.observable("");

	};

	message.subscribe("back", function () {
		this.publish("saveState", ko.toJS(pageModel));
	});

		pageModel = new viewModel();
		ko.applyBindings(pageModel, document.getElementById('jsonConfiguration'));
	
})();