$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var articleService = window.lC.crawler.backOffice.articles.articles;
	
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
                            
                            return window.lC.crawler.backOffice.articles.articles.getMediaLookup;
            }
        });
    });    $('#DataSourceFilterLookupOpenButton').on('click', '', function () {
        lastNpDisplayNameId = 'DataSource_Filter_Url';
        lastNpIdId = 'DataSourceIdFilter';
        _lookupModal.open({
            currentId: $('#DataSourceIdFilter').val(),
            currentDisplayName: $('#DataSource_Filter_Url').val(),
            serviceMethod: function () {
                            
                            return window.lC.crawler.backOffice.articles.articles.getDataSourceLookup;
            }
        });
    });
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "Articles/CreateModal",
        scriptUrl: "/Pages/Articles/createModal.js",
        modalClass: "articleCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "Articles/EditModal",
        scriptUrl: "/Pages/Articles/editModal.js",
        modalClass: "articleEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            title: $("#TitleFilter").val(),
			excerpt: $("#ExcerptFilter").val(),
			content: $("#ContentFilter").val(),
			createdAtMin: $("#CreatedAtFilterMin").data().datepicker.getFormattedDate('yyyy-mm-dd'),
			createdAtMax: $("#CreatedAtFilterMax").data().datepicker.getFormattedDate('yyyy-mm-dd'),
			author: $("#AuthorFilter").val(),
			tags: $("#TagsFilter").val(),
			likeCountMin: $("#LikeCountFilterMin").val(),
			likeCountMax: $("#LikeCountFilterMax").val(),
			commentCountMin: $("#CommentCountFilterMin").val(),
			commentCountMax: $("#CommentCountFilterMax").val(),
			shareCountMin: $("#ShareCountFilterMin").val(),
			shareCountMax: $("#ShareCountFilterMax").val(),
			featuredMediaId: $("#FeaturedMediaIdFilter").val(),			dataSourceId: $("#DataSourceIdFilter").val(),			categoryId: $("#CategoryFilter").val(),			mediaId: $("#MediaFilter").val()
        };
    };

    var dataTable = $("#ArticlesTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(articleService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.Articles.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.article.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.Articles.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    articleService.delete(data.record.article.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "article.title" },
			{ data: "article.excerpt" },
			{ data: "article.content" },
            {
                data: "article.createdAt",
                render: function (createdAt) {
                    if (!createdAt) {
                        return "";
                    }
                    
					var date = Date.parse(createdAt);
                    return (new Date(date)).toLocaleDateString(abp.localization.currentCulture.name);
                }
            },
			{ data: "article.author" },
			{ data: "article.tags" },
			{ data: "article.likeCount" },
			{ data: "article.commentCount" },
			{ data: "article.shareCount" },
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

    $("#NewArticleButton").click(function (e) {
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
                    url: abp.appPath + 'api/app/articles/category-lookup',
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
                    url: abp.appPath + 'api/app/articles/media-lookup',
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
