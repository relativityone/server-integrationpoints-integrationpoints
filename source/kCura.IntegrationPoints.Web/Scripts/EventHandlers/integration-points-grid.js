$(function () {
	//
	//do not use this div for ID's this is just a test to see if I can do this
	$('#_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl02__fieldAttributes_Hidden').parents('table:eq(0)').hide().closest('.editTableColumn').css({
		'background': 'green',
		'height' : '500px'
	}).append('<table id="mappFieldsGrid">Hello world!!</table>');
	debugger;
	//new Dragon.Grid.GridControl({
	//	id: 'mappFieldsGrid',
	//	jsonReaderOptions: {}
	//});
	
});