$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var productService = window.lC.crawler.backOffice.products.products;
	
	
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
			brand: $("#BrandFilter").val(),
			ratingMin: $("#RatingFilterMin").val(),
			ratingMax: $("#RatingFilterMax").val(),
			priceMin: $("#PriceFilterMin").val(),
			priceMax: $("#PriceFilterMax").val(),
			discountPercentMin: $("#DiscountPercentFilterMin").val(),
			discountPercentMax: $("#DiscountPercentFilterMax").val(),
			shortDescription: $("#ShortDescriptionFilter").val(),
			description: $("#DescriptionFilter").val(),
			categoryId: $("#CategoryFilter").val(),			mediaId: $("#MediaFilter").val()
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
			{ data: "product.brand" },
			{ data: "product.rating" },
			{ data: "product.price" },
			{ data: "product.discountPercent" },
			{ data: "product.shortDescription" },
			{ data: "product.description" }
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
