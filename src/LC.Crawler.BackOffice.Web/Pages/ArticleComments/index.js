$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var articleCommentService = window.lC.crawler.backOffice.articleComments.articleComments;
	
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
	    $('#ArticleFilterLookupOpenButton').on('click', '', function () {
        lastNpDisplayNameId = 'Article_Filter_Title';
        lastNpIdId = 'ArticleIdFilter';
        _lookupModal.open({
            currentId: $('#ArticleIdFilter').val(),
            currentDisplayName: $('#Article_Filter_Title').val(),
            serviceMethod: function () {
                            
                            return window.lC.crawler.backOffice.articleComments.articleComments.getArticleLookup;
            }
        });
    });
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "ArticleComments/CreateModal",
        scriptUrl: "/Pages/ArticleComments/createModal.js",
        modalClass: "articleCommentCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "ArticleComments/EditModal",
        scriptUrl: "/Pages/ArticleComments/editModal.js",
        modalClass: "articleCommentEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            name: $("#NameFilter").val(),
			content: $("#ContentFilter").val(),
			likesMin: $("#LikesFilterMin").val(),
			likesMax: $("#LikesFilterMax").val(),
			createdAtMin: $("#CreatedAtFilterMin").data().datepicker.getFormattedDate('yyyy-mm-dd'),
			createdAtMax: $("#CreatedAtFilterMax").data().datepicker.getFormattedDate('yyyy-mm-dd'),
			articleId: $("#ArticleIdFilter").val()
        };
    };

    var dataTable = $("#ArticleCommentsTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(articleCommentService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.ArticleComments.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.articleComment.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.ArticleComments.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    articleCommentService.delete(data.record.articleComment.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "articleComment.name" },
			{ data: "articleComment.content" },
			{ data: "articleComment.likes" },
            {
                data: "articleComment.createdAt",
                render: function (createdAt) {
                    if (!createdAt) {
                        return "";
                    }
                    
					var date = Date.parse(createdAt);
                    return (new Date(date)).toLocaleDateString(abp.localization.currentCulture.name);
                }
            },
            {
                data: "article.title",
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

    $("#NewArticleCommentButton").click(function (e) {
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
