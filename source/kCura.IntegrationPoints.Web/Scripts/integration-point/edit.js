(function (root) {
	$('#stepProgress').stepProgress({
		steps: [
			{
				text: 'Set Integration Details'
			},
			{
				text: 'Map Fields'
			},
			{
				text: 'Preview Import'
			}
		]
	});
	$('#next').on('click', function () {
		root.messaging.publish('next');
	});
	
	$('#back').on('click', function () {
		root.messaging.publish('back');
	});

	root.messaging.subscribe("goToStep", function (step) {
		$('#stepProgress').stepProgress('goToStep', step +1);
	});

})(IP);
