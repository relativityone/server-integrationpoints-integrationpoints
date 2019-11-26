$(function () {
	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging();

	var _getModel = function () {
	    return $('#fileLocation').val();
	};

	//An event raised when the user has clicked the Next or Save button.
	message.subscribe('submit', function () {
		//Execute save logic that persists the state.
		var localModel = _getModel();
		this.publish("saveState", localModel);
		//Communicate to the host page that it to continue.
		this.publish('saveComplete', localModel);
	});

	//An event raised when a user clicks the Back button.
	message.subscribe('back', function () {
		//Execute save logic that persists the state.
		this.publish('saveState', _getModel());
	});

	//An event raised when the host page has loaded the current settings page.
	message.subscribe('load', function (model) {
		$('#fileLocation').val(model);
	});
});