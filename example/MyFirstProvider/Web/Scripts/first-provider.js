$(function () {
	//create a new communication object to talk to the host page.
	var message = IP.frameMessaging();

	var _getModel = function () {
		return $('#fileLocation').val();
	};

	//raised when the user has clicked the submit button
	message.subscribe('submit', function () {
		//do any save logic to persist the state here
		var localModel = _getModel();
		this.publish("saveState", localModel);
		//tell the host page that it is ok to continue
		this.publish('saveComplete', localModel);
	});

	//raised when a user clicks the back button
	message.subscribe('back', function () {
		//do any save logic to persist the state here
		this.publish('saveState', _getModel());
	});

	//raised when the host page has loaded the current settings page.
	message.subscribe('load', function (model) {
		$('#fileLocation').val(model);
	});
});