$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var productService = window.lC.crawler.backOffice.products.products;
	
        var lastNpIdId = '';
        var lastNpDisplayNameId = '';

        var _lookupModal = new abp.ModalManager({
            viewUrl: abp.appPath + "Shared/LookupModal",
            scriptUrl: "/Pages/Shared/lookupModal.js",
            modalClass: "navigationPropertyLookup"
        });

        $('.lookupCleanButton').on('click', '', function () {
            $(this).parent().find('input').val('');
        });

        _lookupModal.onClose(function () {
            var modal = $(_lookupModal.getModal());
            $('#' + lastNpIdId).val(modal.find('#CurrentLookupId').val());
            $('#' + lastNpDisplayNameId).val(modal.find('#CurrentLookupDisplayName').val());
        });
	    $('#MediaFilterLookupOpenButton').on('click', '', function () {
        lastNpDisplayNameId = 'Media_Filter_Url';
        lastNpIdId = 'MediaIdFilter';
        _lookupModal.open({
            currentId: $('#MediaIdFilter').val(),
            currentDisplayName: $('#Media_Filter_Url').val(),
            serviceMethod: function () {
                            
                            return window.lC.crawler.backOffice.products.products.getMediaLookup;
            }
        });
    });
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "Products/CreateModal",
        scriptUrl: "/Pages/Products/createModal.js",
        modalClass: "productCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "Products/EditModal",
        scriptUrl: "/Pages/Products/editModal.js",
        modalClass: "productEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            name: $("#NameFilter").val(),
			code: $("#CodeFilter").val(),
			shortDescription: $("#ShortDescriptionFilter").val(),
			description: $("#DescriptionFilter").val(),
			externalIdMin: $("#ExternalIdFilterMin").val(),
			externalIdMax: $("#ExternalIdFilterMax").val(),
			featuredMediaId: $("#FeaturedMediaIdFilter").val(),			dataSourceId: $("#DataSourceIdFilter").val(),			categoryId: $("#CategoryFilter").val(),			mediaId: $("#MediaFilter").val()
        };
    };

    var dataTable = $("#ProductsTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(productService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.Products.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.product.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.Products.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    productService.delete(data.record.product.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "product.name" },
			{ data: "product.code" },
			{ data: "product.shortDescription" },
			{ data: "product.description" },
			{ data: "product.externalId" },
            {
                data: "media.url",
                defaultContent : "", 
                orderable: false
            },
            {
                data: "dataSource.url",
                defaultContent : "", 
                orderable: false
            }
        ]
    }));

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $("#NewProductButton").click(function (e) {
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
    
                $('#CategoryFilter').select2({
                ajax: {
                    url: abp.appPath + 'api/app/products/category-lookup',
                    type: 'GET',
                    data: function (params) {
                        return { filter: params.term, maxResultCount: 10 }
                    },
                    processResults: function (data) {
                        var mappedItems = _.map(data.items, function (item) {
                            return { id: item.id, text: item.displayName };
                        });
                        mappedItems.unshift({ id: "", text: ' - ' });

                        return { results: mappedItems };
                    }
                }
            });
                    $('#MediaFilter').select2({
                ajax: {
                    url: abp.appPath + 'api/app/products/media-lookup',
                    type: 'GET',
                    data: function (params) {
                        return { filter: params.term, maxResultCount: 10 }
                    },
                    processResults: function (data) {
                        var mappedItems = _.map(data.items, function (item) {
                            return { id: item.id, text: item.displayName };
                        });
                        mappedItems.unshift({ id: "", text: ' - ' });

                        return { results: mappedItems };
                    }
                }
            });
        
});
