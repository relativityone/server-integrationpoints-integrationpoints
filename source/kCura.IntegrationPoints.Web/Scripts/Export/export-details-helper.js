var ExportDetailsHelper = {};

ExportDetailsHelper.modifySummaryPage = function (settings, $rootField) {
    IP.utils.getViewField(IP.overwriteFields).closest('.editTableColumn').parent().hide();
    IP.utils.getViewField(IP.sourceConfiguration).closest('.editTableColumn').parent().hide();
    IP.utils.getViewField(IP.destinationid).closest('.editTableColumn').parent().hide();
    IP.data.ajax({
        url: IP.utils.generateWebURL('IntegrationPoints', 'ExportDetails'),
        type: 'post',
        data: settings,
        dataType: 'html'
    }).then(function (result) {
        $rootField.closest('.innerTabTable').children('tbody').append(result);
    });
}
