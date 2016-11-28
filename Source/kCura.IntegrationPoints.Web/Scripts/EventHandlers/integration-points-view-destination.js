var IP = IP || {};
IP.utils = IP.utils || {};

IP.utils.getViewField = function (id) {
	return $('input[faartifactid="' + id + '"]');
};
IP.utils.updateField = function ($el, text, value) {
	$el.find('.dynamicViewFieldName').text(text + ':');
	$el.find('.dynamicViewFieldValue').html(value);
	return $el;
};

$(function () {
	
	var destinationId = IP.destinationid;
	var $input = IP.utils.getViewField(destinationId);
	var $value = $input.siblings('.dynamicViewFieldValue');
	$input.siblings('.dynamicViewFieldName').text();
	var obj = JSON.parse($value.text());

	IP.utils.updateField($input.parent('tr'), 'Destination RDO', obj.ArtifactTypeName);
});

$(function () {

	IP.utils.getViewField(IP.destinationProviderid).siblings('.dynamicViewFieldValue').find('a').replaceWith(function(){
		return '<span>' + $(this).text() + '</span';
	});

	IP.utils.getViewField(IP.sourceProviderId).siblings('.dynamicViewFieldValue').find('a').replaceWith(function () {
		return '<span>' + $(this).text() + '</span';
	});

});