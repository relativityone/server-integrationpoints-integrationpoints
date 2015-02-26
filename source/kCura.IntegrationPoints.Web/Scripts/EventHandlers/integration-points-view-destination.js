var IP = IP || {};
IP.utils = IP.utils || {};

IP.utils.getViewField = function (id) {
	return $('input[faartifactid="' + id + '"]');
}

$(function () {
	

	var destinationId = IP.destinationid;
	var $input = IP.utils.getViewField(destinationId);
	var $value = $input.siblings('.dynamicViewFieldValue');
	$input.siblings('.dynamicViewFieldName').text('Destination Rdo:');
	var obj = JSON.parse($value.text());
	$value.text('');
	var url = IP.utils.generateWebAPIURL('RdoFilter', obj.artifactTypeID);


	IP.data.get(url).then(function (result) {
		$value.text(result.name);
	}, function () {
		//debugger;
	});
});

$(function () {

	IP.utils.getViewField(IP.destinationProviderid).siblings('.dynamicViewFieldValue').find('a').replaceWith(function(){
		return '<span>' + $(this).text() + '</span';
	});
})();