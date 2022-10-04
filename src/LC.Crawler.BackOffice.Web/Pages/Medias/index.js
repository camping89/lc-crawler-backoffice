$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var mediaService = window.lC.crawler.backOffice.medias.medias;
	
	
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "Medias/CreateModal",
        scriptUrl: "/Pages/Medias/createModal.js",
        modalClass: "mediaCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "Medias/EditModal",
        scriptUrl: "/Pages/Medias/editModal.js",
        modalClass: "mediaEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            name: $("#NameFilter").val(),
			contentType: $("#ContentTypeFilter").val(),
			url: $("#UrlFilter").val(),
			description: $("#DescriptionFilter").val(),
            isDowloaded: (function () {
                var value = $("#IsDowloadedFilter").val();
                if (value === undefined || value === null || value === '') {
                    return '';
                }
                return value === 'true';
            })()
        };
    };

    var dataTable = $("#MediasTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(mediaService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.Medias.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.Medias.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    mediaService.delete(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "name" },
			{ data: "contentType" },
			{ data: "url" },
			{ data: "description" },
            {
                data: "isDowloaded",
                render: function (isDowloaded) {
                    return isDowloaded ? l("Yes") : l("No");
                }
            }
        ]
    }));

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $("#NewMediaButton").click(function (e) {
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
