$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var productVariantService = window.lC.crawler.backOffice.productVariants.productVariants;
	
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
	
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "ProductVariants/CreateModal",
        scriptUrl: "/Pages/ProductVariants/createModal.js",
        modalClass: "productVariantCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "ProductVariants/EditModal",
        scriptUrl: "/Pages/ProductVariants/editModal.js",
        modalClass: "productVariantEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            sKU: $("#SKUFilter").val(),
			retailPriceMin: $("#RetailPriceFilterMin").val(),
			retailPriceMax: $("#RetailPriceFilterMax").val(),
			discountRateMin: $("#DiscountRateFilterMin").val(),
			discountRateMax: $("#DiscountRateFilterMax").val(),
			discountedPriceMin: $("#DiscountedPriceFilterMin").val(),
			discountedPriceMax: $("#DiscountedPriceFilterMax").val(),
			productId: $("#ProductIdFilter").val()
        };
    };

    var dataTable = $("#ProductVariantsTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(productVariantService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.ProductVariants.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.productVariant.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.ProductVariants.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    productVariantService.delete(data.record.productVariant.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "productVariant.sku" },
			{ data: "productVariant.retailPrice" },
			{ data: "productVariant.discountRate" },
			{ data: "productVariant.discountedPrice" },
            {
                data: "product.name",
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

    $("#NewProductVariantButton").click(function (e) {
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
