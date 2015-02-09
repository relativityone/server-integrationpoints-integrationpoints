var IP = IP || {};

$(function () {
	var message = IP.frameMessaging();

	var _getModel = function () {
		return $('#fileLocation').val()
	};


	message.subscribe('submit', function () {
		var localModel = _getModel();
		this.publish("saveState", localModel);
		this.publish('saveComplete', localModel);
	});

	message.subscribe('back', function () {
		this.publish('saveState', _getModel());
	});
	
	message.subscribe('load', function (model) {
		$('#fileLocation').val(model);
	});
});