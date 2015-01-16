
var IP  = IP || {};

(function () {
	
	var message = IP.frameMessaging();
	var pageModel = {};
	var self = this;
	message.subscribe('submit', function () {
		
		var localModel = ko.toJS(pageModel);
		this.publish("saveState", localModel); //save the model incase of error
		var self = this;

		
		$.ajax({
			url: IP.utils.generateWebAPIURL('SourceFields/'),
			type: "POST",
			data: JSON.stringify(localModel),
			success: function (data, textStatus, jqXHR) {
				self.publish('saveComplete', ko.toJS(pageModel));
			},
			error: function (jqXHR, textStatus, errorThrown) {

			}
		});
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