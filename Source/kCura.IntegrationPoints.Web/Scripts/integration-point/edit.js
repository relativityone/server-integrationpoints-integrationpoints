(function (root) {
	$('#next').on('click', function () {
		root.messaging.publish('next');
	});
	
	$('#back').on('click', function () {
		root.messaging.publish('back');
	});

	root.messaging.subscribe("goToStep", function (step) {
		$('#stepProgress').stepProgress('goToStep', step + 1);
	});

	root.messaging.subscribe("goToStep", function (step) {
		if ($('#stepProgress').stepProgress('last')) {
			$('#next').hide();
			$('#save').show();
		} else {
			$('#next').show();
			$('#save').hide();
		}
	});
	
	$('#save').on('click', function () {
	    var save = $(this).attr('save');
	    if (typeof (save) == 'undefined') {
	        IP.messaging.publish('save');
	    }
	});



})(IP);
