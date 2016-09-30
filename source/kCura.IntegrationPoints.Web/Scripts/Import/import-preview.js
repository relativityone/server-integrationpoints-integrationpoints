var IP = IP || {};

(function (root) {

    $.get(root.utils.getBaseURL() + "/api/ImportPreview/DummbyData/")
        .done(function (data) {
            // data = $.parseJSON(data);
            var formattedHeaders = [];
            $.each(data.Headers, function (e, f) {
                formattedHeaders.push({ "sTitle": data.Headers[e] });
            });

            console.log(data);

            var csvTable = $("#csvData-table").DataTable({
                "bFilter": false,
                "bInfo": false,
                "destroy": false,
                "paging": true,
                "lengthChange": false,
                "responsive": true,
                "info": true,
                "pageLength": 4,
                "columns": formattedHeaders

            });

            csvTable.clear();
            csvTable.draw();

            csvTable.rows.add(
                data.Data
            );

            csvTable.draw();

            /*Todo check to see if there is a way to enable "items for page" without setting to true*/
            $("#csvData-table_paginate").hide();

            $("#pag-nav-move-previous").on("click", function () {
                csvTable.page('previous').draw('page');
            });

            $("#pag-nav-move-next").on("click", function () {
                csvTable.page('next').draw('page');
            });

            $("#pag-nav-move-first").on("click", function () {
                csvTable.page('first').draw('page');
            });

            $("#pag-nav-move-last").on("click", function () {
                csvTable.page('last').draw('page');
            });

            $("#itemsPerPageSelect").live("click", function () {
                var pageLength = $("#itemsPerPageSelect option:selected").val();
                console.log(pageLength);
                csvTable.page.len(pageLength).draw();
            });
        });
})(IP);
