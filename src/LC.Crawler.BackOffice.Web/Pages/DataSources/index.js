$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var dataSourceService = window.lC.crawler.backOffice.dataSources.dataSources;
	
	
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "DataSources/CreateModal",
        scriptUrl: "/Pages/DataSources/createModal.js",
        modalClass: "dataSourceCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "DataSources/EditModal",
        scriptUrl: "/Pages/DataSources/editModal.js",
        modalClass: "dataSourceEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            url: $("#UrlFilter").val(),
            isActive: (function () {
                var value = $("#IsActiveFilter").val();
                if (value === undefined || value === null || value === '') {
                    return '';
                }
                return value === 'true';
            })(),
			postToSite: $("#PostToSiteFilter").val()
        };
    };

    var dataTable = $("#DataSourcesTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(dataSourceService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.DataSources.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.DataSources.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    dataSourceService.delete(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "url" },
            {
                data: "isActive",
                render: function (isActive) {
                    return isActive ? l("Yes") : l("No");
                }
            },
			{ data: "postToSite" }
        ]
    }));

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $("#NewDataSourceButton").click(function (e) {
        e.preventDefault();
        createModal.open();
    });

	$("#SearchForm").submit(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $('#AdvancedFilterSectionToggler').on('click', function (e) {
        $('#AdvancedFilterSection').toggle();
    });

    $('#AdvancedFilterSection').on('keypress', function (e) {
        if (e.which === 13) {
            dataTable.ajax.reload();
        }
    });

    $('#AdvancedFilterSection select').change(function() {
        dataTable.ajax.reload();
    });
    
    
});
